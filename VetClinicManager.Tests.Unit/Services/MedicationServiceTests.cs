using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Data;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class MedicationServiceTests
{
    private ApplicationDbContext _context = null!;
    private MedicationService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new MedicationService(
            _context,
            new MedicationMapper(),
            new MedicationBriefMapper());
    }

    [TearDown]
    public void TearDown() => _context.Dispose();
    
    [Test]
    public async Task DeleteMedicationAsync_WhenLinkedToPrescription_ShouldReturnFalseAndNotRemove()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Models.Enums.Gender.Male, Owner = owner };
        var medication = new Medication { Name = "Amoxicillin", Description = "Antibiotic" };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        var visit = new Visit
        {
            Title = "Check",
            ScheduledAt = DateTime.UtcNow,
            Status = Models.Enums.VisitStatus.Scheduled,
            Priority = Models.Enums.VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = owner.Id
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        var visitUpdate = new VisitUpdate
        {
            VisitId = visit.Id,
            UpdatedByVetId = owner.Id,
            Notes = "n",
            UpdateDate = DateTime.UtcNow
        };
        _context.VisitUpdates.Add(visitUpdate);
        await _context.SaveChangesAsync();

        var prescription = new Prescription
        {
            MedicationId = medication.Id,
            Dosage = "2 times a day",
            VisitUpdateId = visitUpdate.Id
        };
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteMedicationAsync(medication.Id);

        result.Should().BeFalse();
        _context.Medications.Should().ContainSingle(m => m.Id == medication.Id);
    }

    [Test]
    public async Task DeleteMedicationAsync_WhenNoLinks_ShouldReturnTrueAndRemoveEntity()
    {
        var medication = new Medication { Name = "Amoxicillin", Description = "Antibiotic" };
        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteMedicationAsync(medication.Id);

        result.Should().BeTrue();
        _context.Medications.Should().BeEmpty();
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