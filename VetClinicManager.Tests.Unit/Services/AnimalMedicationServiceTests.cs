using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class AnimalMedicationServiceTests
{
    private ApplicationDbContext _context = null!;
    private AnimalMedicationService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new AnimalMedicationService(_context, new AnimalMedicationMapper());
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task SyncPrescriptionDeletedAsync_WhenNoMatchingPrescriptionId_ShouldNotRemoveUnrelatedRows()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var med = new Medication { Name = "Drug", Description = "D" };
        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.Medications.AddAsync(med);
        await _context.SaveChangesAsync();

        var unrelated = new AnimalMedication
        {
            AnimalId = animal.Id,
            MedicationId = med.Id,
            StartDate = DateTime.UtcNow.Date,
            PrescriptionId = 1001
        };
        _context.AnimalMedications.Add(unrelated);
        await _context.SaveChangesAsync();

        var act = async () => await _service.SyncPrescriptionDeletedAsync(99999);

        await act.Should().NotThrowAsync();
        (await _context.AnimalMedications.AnyAsync(am => am.Id == unrelated.Id)).Should().BeTrue();
        (await _context.AnimalMedications.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task SyncPrescriptionAddedAsync_WhenAnimalAndMedicationValid_ShouldPersistAnimalMedication()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var med = new Medication { Name = "Drug", Description = "D" };
        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.Medications.AddAsync(med);
        await _context.SaveChangesAsync();

        await _service.SyncPrescriptionAddedAsync(animal.Id, med.Id, DateTime.UtcNow.Date, prescriptionId: 5001);

        var row = await _context.AnimalMedications.SingleAsync();
        row.AnimalId.Should().Be(animal.Id);
        row.MedicationId.Should().Be(med.Id);
        row.PrescriptionId.Should().Be(5001);
    }

    [Test]
    public async Task CreateAnimalMedicationAsync_WhenDtoValid_ShouldPersistAndReturnId()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var med = new Medication { Name = "M", Description = "D" };
        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.Medications.AddAsync(med);
        await _context.SaveChangesAsync();

        var dto = new AnimalMedicationCreateDto
        {
            AnimalId = animal.Id,
            HealthRecordId = 0,
            MedicationId = med.Id,
            StartDate = DateTime.UtcNow.Date
        };

        var id = await _service.CreateAnimalMedicationAsync(dto);

        id.Should().BeGreaterThan(0);
        (await _context.AnimalMedications.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task GetForCreateAsync_WhenAnimalIdUnknown_ShouldReturnNull()
    {
        var dto = await _service.GetForCreateAsync(99999);

        dto.Should().BeNull();
    }

    [Test]
    public async Task DeleteAnimalMedicationAsync_WhenIdExists_ShouldRemoveRow()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        var med = new Medication { Name = "M", Description = "D" };
        await _context.Users.AddAsync(owner);
        await _context.Animals.AddAsync(animal);
        await _context.Medications.AddAsync(med);
        await _context.SaveChangesAsync();

        var am = new AnimalMedication
        {
            AnimalId = animal.Id,
            MedicationId = med.Id,
            StartDate = DateTime.UtcNow.Date
        };
        _context.AnimalMedications.Add(am);
        await _context.SaveChangesAsync();

        var ok = await _service.DeleteAnimalMedicationAsync(am.Id);

        ok.Should().BeTrue();
        (await _context.AnimalMedications.CountAsync()).Should().Be(0);
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