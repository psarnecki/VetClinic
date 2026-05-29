using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Prescriptions;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class VisitUpdateServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IAnimalMedicationService> _mockAnimalMedication = null!;
    private Mock<IFileService> _mockFile = null!;
    private VisitUpdateService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockAnimalMedication = new Mock<IAnimalMedicationService>();
        _mockFile = new Mock<IFileService>();

        _service = new VisitUpdateService(
            _context,
            new VisitUpdateMapper(),
            _mockAnimalMedication.Object,
            _mockFile.Object);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private void SeedUsersAnimal(User owner, User vetA, User vetB, Animal animal)
    {
        _context.Users.AddRange(owner, vetA, vetB);
        _context.Animals.Add(animal);
        _context.SaveChanges();
    }

    [Test]
    public async Task GetForEditAsync_WhenWrongVetAndNotAdmin_ShouldThrowUnauthorizedAccessException()
    {
        var vetA = NewUser("a", "A", "V");
        var vetB = NewUser("b", "B", "V");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        SeedUsersAnimal(owner, vetA, vetB, animal);

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vetA.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vetA.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var act = async () => await _service.GetForEditAsync(vu.Id, vetB.Id, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task GetForDeleteAsync_WhenWrongVetAndNotAdmin_ShouldReturnNull()
    {
        var vetA = NewUser("a", "A", "V");
        var vetB = NewUser("b", "B", "V");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        SeedUsersAnimal(owner, vetA, vetB, animal);

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vetA.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vetA.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var dto = await _service.GetForDeleteAsync(vu.Id, vetB.Id, isAdmin: false);

        dto.Should().BeNull();
    }

    [Test]
    public async Task GetForCreateAsync_WhenVisitIdUnknown_ShouldReturnNull()
    {
        var dto = await _service.GetForCreateAsync(99999);

        dto.Should().BeNull();
    }

    [Test]
    public async Task DeleteVisitUpdateAsync_WhenWrongVetAndNotAdmin_ShouldThrowUnauthorizedAccessException()
    {
        var vetA = NewUser("a", "A", "V");
        var vetB = NewUser("b", "B", "V");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        SeedUsersAnimal(owner, vetA, vetB, animal);

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vetA.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vetA.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var act = async () => await _service.DeleteVisitUpdateAsync(vu.Id, vetB.Id, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task CreateVisitUpdateAsync_WhenNoImageAndNoPrescriptions_ShouldReturnVisitId()
    {
        var vet = NewUser("v", "V", "T");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        _context.Users.AddRange(vet, owner);
        _context.Animals.Add(animal);
        _context.SaveChanges();

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var createDto = new VisitUpdateCreateDto
        {
            VisitId = visit.Id,
            Notes = "Note text"
        };

        var visitId = await _service.CreateVisitUpdateAsync(createDto, vet.Id);

        visitId.Should().Be(visit.Id);
        (await _context.VisitUpdates.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task GetForEditAsync_WhenAdmin_ShouldReturnDtoEvenIfNotAuthorVet()
    {
        var vetA = NewUser("a", "A", "V");
        var vetB = NewUser("b", "B", "V");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        SeedUsersAnimal(owner, vetA, vetB, animal);

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vetA.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vetA.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var dto = await _service.GetForEditAsync(vu.Id, vetB.Id, isAdmin: true);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(vu.Id);
    }

    [Test]
    public async Task CreateVisitUpdateAsync_WhenHasPrescriptions_ShouldCallSyncAddedForEachPrescription()
    {
        var vet = NewUser("v", "V", "T");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var med1 = new Medication { Name = "M1", Description = "D" };
        var med2 = new Medication { Name = "M2", Description = "D" };
        _context.Users.AddRange(vet, owner);
        _context.Animals.Add(animal);
        _context.Medications.AddRange(med1, med2);
        _context.SaveChanges();

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var createDto = new VisitUpdateCreateDto
        {
            VisitId = visit.Id,
            Notes = "with prescriptions",
            Prescriptions = new List<PrescriptionCreateDto>
            {
                new() { MedicationId = med1.Id, Dosage = "1x" },
                new() { MedicationId = med2.Id, Dosage = "2x" }
            }
        };

        var visitId = await _service.CreateVisitUpdateAsync(createDto, vet.Id);

        visitId.Should().Be(visit.Id);
        _mockAnimalMedication.Verify(
            m => m.SyncPrescriptionAddedAsync(animal.Id, med1.Id, It.IsAny<DateTime>(), It.IsAny<int>()),
            Times.Once);
        _mockAnimalMedication.Verify(
            m => m.SyncPrescriptionAddedAsync(animal.Id, med2.Id, It.IsAny<DateTime>(), It.IsAny<int>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateVisitUpdateAsync_WhenPrescriptionsReplaced_ShouldSyncOldDeletedAndNewAdded()
    {
        var vet = NewUser("v", "V", "T");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var oldMed = new Medication { Name = "Old", Description = "D" };
        var newMed = new Medication { Name = "New", Description = "D" };
        _context.Users.AddRange(vet, owner);
        _context.Animals.Add(animal);
        _context.Medications.AddRange(oldMed, newMed);
        _context.SaveChanges();

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vet.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var oldPrescription = new Prescription { Dosage = "1x", MedicationId = oldMed.Id, VisitUpdateId = vu.Id };
        _context.Prescriptions.Add(oldPrescription);
        _context.SaveChanges();
        var oldPrescriptionId = oldPrescription.Id;

        var editDto = new VisitUpdateEditDto
        {
            Id = vu.Id,
            VisitId = visit.Id,
            Notes = "updated",
            Prescriptions = new List<PrescriptionEditDto>
            {
                new() { MedicationId = newMed.Id, Dosage = "3x" }
            }
        };

        var visitId = await _service.UpdateVisitUpdateAsync(editDto, vet.Id, isAdmin: false);

        visitId.Should().Be(visit.Id);
        _mockAnimalMedication.Verify(m => m.SyncPrescriptionDeletedAsync(oldPrescriptionId), Times.Once);
        _mockAnimalMedication.Verify(
            m => m.SyncPrescriptionAddedAsync(animal.Id, newMed.Id, It.IsAny<DateTime>(), It.IsAny<int>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateVisitUpdateAsync_WhenIdUnknown_ShouldThrowKeyNotFoundException()
    {
        var editDto = new VisitUpdateEditDto { Id = 99999, Notes = "x" };

        var act = async () => await _service.UpdateVisitUpdateAsync(editDto, "any-vet", isAdmin: true);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task UpdateVisitUpdateAsync_WhenRemoveImageFlagSet_ShouldDeleteFileAndClearImageUrl()
    {
        var vet = NewUser("v", "V", "T");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        _context.Users.AddRange(vet, owner);
        _context.Animals.Add(animal);
        _context.SaveChanges();

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        };
        _context.Visits.Add(visit);
        _context.SaveChanges();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vet.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow,
            ImageUrl = "/uploads/visit-updates/old.jpg"
        };
        _context.VisitUpdates.Add(vu);
        _context.SaveChanges();

        var editDto = new VisitUpdateEditDto
        {
            Id = vu.Id,
            VisitId = visit.Id,
            Notes = "n",
            RemoveImage = true
        };

        await _service.UpdateVisitUpdateAsync(editDto, vet.Id, isAdmin: false);

        _mockFile.Verify(f => f.DeleteFile("/uploads/visit-updates/old.jpg"), Times.Once);
        var reloaded = await _context.VisitUpdates.AsNoTracking().SingleAsync(x => x.Id == vu.Id);
        reloaded.ImageUrl.Should().BeNull();
    }

    private static User NewUser(string id, string f, string l) => new()
    {
        Id = id,
        UserName = $"{f}.{l}@test.local",
        Email = $"{f}.{l}@test.local",
        FirstName = f,
        LastName = l
    };
}