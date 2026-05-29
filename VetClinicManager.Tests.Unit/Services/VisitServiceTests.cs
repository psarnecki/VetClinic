using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestPDF.Infrastructure;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class VisitServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<IAnimalMedicationService> _mockAnimalMedicationService = null!;
    private VisitService _service = null!;

    [SetUp]
    public void Setup()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mockAnimalMedicationService = new Mock<IAnimalMedicationService>();

        _service = new VisitService(
            _context,
            new VisitMapper(),
            new AnimalBriefMapper(),
            new UserBriefMapper(),
            _mockUserManager.Object,
            _mockAnimalMedicationService.Object
        );
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetOpenVisitsForReportAsync_ShouldReturnOnlyTodayScheduledOrInProgressVisits()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        var vet = CreateUser("vet1", "Dr", "House");
        var owner = CreateUser("owner1", "John", "Doe");
        var animal = new Animal { Name = "Doggo", Owner = owner, BodyWeight = 12f, Gender = Gender.Male };

        var visits = new List<Visit>
        {
            new Visit
            {
                Id = 1,
                Title = "A",
                ScheduledAt = today.AddHours(10),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            },
            new Visit
            {
                Id = 2,
                Title = "B",
                ScheduledAt = today.AddHours(11),
                Status = VisitStatus.InProgress,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            },
            new Visit
            {
                Id = 3,
                Title = "C",
                ScheduledAt = today.AddHours(12),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            },
            new Visit
            {
                Id = 4,
                Title = "D",
                ScheduledAt = today.AddHours(13),
                Status = VisitStatus.Cancelled,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            },
            new Visit
            {
                Id = 5,
                Title = "E",
                ScheduledAt = yesterday.AddHours(10),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            },
            new Visit
            {
                Id = 6,
                Title = "F",
                ScheduledAt = tomorrow.AddHours(10),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                Animal = animal,
                AssignedVet = vet
            }
        };

        await _context.Users.AddRangeAsync(vet, owner);
        await _context.Animals.AddAsync(animal);
        await _context.Visits.AddRangeAsync(visits);
        await _context.SaveChangesAsync();

        var result = await _service.GetOpenVisitsForReportAsync();

        result.Should().HaveCount(2);
        result.Select(v => v.Id).Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Test]
    public async Task CreateVisitAsync_ShouldPersistVisitAndReturnGeneratedId()
    {
        var owner = CreateUser("owner1", "Anne", "Miller");
        var animal = new Animal
        {
            Name = "Whiskers",
            BodyWeight = 4.5f,
            Gender = Gender.Female,
            Owner = owner
        };

        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var createDto = new VisitCreateDto
        {
            Title = "Vaccination",
            ScheduledAt = DateTime.UtcNow.AddDays(2),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id
        };

        var newId = await _service.CreateVisitAsync(createDto);

        newId.Should().BeGreaterThan(0);

        var fromDb = await _context.Visits.AsNoTracking().SingleOrDefaultAsync(v => v.Id == newId);
        fromDb.Should().NotBeNull();
        fromDb!.Title.Should().Be("Vaccination");
        fromDb.AnimalId.Should().Be(animal.Id);
        fromDb.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GeneratePdfReportAsync_WhenVisitIdNotFound_ShouldReturnNull()
    {
        var result = await _service.GeneratePdfReportAsync(99999, "u1", new List<string> { "Admin" });

        result.Should().BeNull();
    }

    [Test]
    public async Task GeneratePdfReportAsync_WhenClientOwnerMatchesOwnerId_ShouldReturnNonEmptyPdf()
    {
        var owner = CreateUser("owner-1", "Olivia", "Owner");
        var animal = new Animal { Name = "Dog", BodyWeight = 5f, Gender = Gender.Male, Owner = owner };
        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var visit = new Visit
        {
            Title = "Check",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = null
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        var result = await _service.GeneratePdfReportAsync(visit.Id, owner.Id, new List<string> { "Client" });

        result.Should().NotBeNull();
        result!.Value.FileContents.Should().NotBeEmpty();
        result.Value.FileName.Should().Contain("Visit_Report");
    }

    [Test]
    public async Task GeneratePdfReportAsync_WhenAdmin_ShouldAccessVisitAssignedToAnotherVet()
    {
        var owner = CreateUser("o1", "O", "W");
        var vet = CreateUser("vet-1", "V", "T");
        var animal = new Animal { Name = "A", BodyWeight = 1f, Gender = Gender.Female, Owner = owner };
        await _context.Users.AddRangeAsync(owner, vet);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var visit = new Visit
        {
            Title = "V",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        var result = await _service.GeneratePdfReportAsync(visit.Id, "admin-user", new List<string> { "Admin" });

        result.Should().NotBeNull();
        result!.Value.FileContents.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetDetailsForOwnerAsync_WhenOwnerIdWrong_ShouldReturnNull()
    {
        var realOwner = CreateUser("real", "R", "O");
        var otherOwner = CreateUser("other", "O", "O");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = realOwner };
        await _context.Users.AddRangeAsync(realOwner, otherOwner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var visit = new Visit
        {
            Title = "T",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        var dto = await _service.GetDetailsForOwnerAsync(visit.Id, otherOwner.Id);

        dto.Should().BeNull();
    }

    [Test]
    public async Task GetForEditAsync_WhenVetNotAssignedToVisit_ShouldReturnNull()
    {
        var vetA = CreateUser("vet-a", "A", "V");
        var vetB = CreateUser("vet-b", "B", "V");
        var owner = CreateUser("own", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        await _context.Users.AddRangeAsync(vetA, vetB, owner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

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
        await _context.SaveChangesAsync();

        var dto = await _service.GetForEditAsync(visit.Id, vetB.Id, isVet: true);

        dto.Should().BeNull();
    }

    [Test]
    public async Task UpdateVisitAsync_WhenVetNotAssignedToVisit_ShouldReturnFalse()
    {
        var vetA = CreateUser("vet-a", "A", "V");
        var vetB = CreateUser("vet-b", "B", "V");
        var owner = CreateUser("own", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        await _context.Users.AddRangeAsync(vetA, vetB, owner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

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
        await _context.SaveChangesAsync();

        var edit = new VisitEditDto
        {
            Id = visit.Id,
            Title = "Updated",
            ScheduledAt = visit.ScheduledAt,
            Status = visit.Status,
            Priority = visit.Priority,
            AssignedVetId = vetA.Id
        };

        var ok = await _service.UpdateVisitAsync(visit.Id, edit, vetB.Id, isVet: true);

        ok.Should().BeFalse();
    }

    [Test]
    public async Task GetVisitsForStaffAsync_WhenVetIdProvided_ShouldReturnOnlyVisitsAssignedToThatVet()
    {
        var vet1 = CreateUser("v1", "V", "One");
        var vet2 = CreateUser("v2", "V", "Two");
        var owner = CreateUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        await _context.Users.AddRangeAsync(vet1, vet2, owner);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        _context.Visits.Add(new Visit
        {
            Title = "A",
            ScheduledAt = DateTime.UtcNow.AddHours(1),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet1.Id
        });
        _context.Visits.Add(new Visit
        {
            Title = "B",
            ScheduledAt = DateTime.UtcNow.AddHours(2),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet2.Id
        });
        await _context.SaveChangesAsync();

        var list = (await _service.GetVisitsForStaffAsync(vet1.Id, null)).ToList();

        list.Should().HaveCount(1);
        list[0].Title.Should().Be("A");
    }

    [Test]
    public async Task DeleteVisitAsync_WhenVisitHasPrescriptions_ShouldCallSyncPrescriptionDeletedForEach()
    {
        var med = new Medication { Name = "Med", Description = "D" };
        var owner = CreateUser("o", "O", "W");
        var vet = CreateUser("v", "V", "T");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        await _context.Medications.AddAsync(med);
        await _context.Users.AddRangeAsync(owner, vet);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

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
        await _context.SaveChangesAsync();

        var vu = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = vet.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(vu);
        await _context.SaveChangesAsync();

        var rx = new Prescription
        {
            Dosage = "10mg",
            MedicationId = med.Id,
            VisitUpdateId = vu.Id
        };
        _context.Prescriptions.Add(rx);
        await _context.SaveChangesAsync();

        var deleted = await _service.DeleteVisitAsync(visit.Id);

        deleted.Should().BeTrue();
        _mockAnimalMedicationService.Verify(m => m.SyncPrescriptionDeletedAsync(rx.Id), Times.Once);
    }

    private static User CreateUser(string id, string firstName, string lastName)
    {
        return new User
        {
            Id = id,
            UserName = $"{firstName}.{lastName}@test.local",
            Email = $"{firstName}.{lastName}@test.local",
            FirstName = firstName,
            LastName = lastName
        };
    }
}