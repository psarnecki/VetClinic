using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestPDF.Infrastructure;
using VetClinicManager.Data;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class VisitServicePdfAuthorizationTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IAnimalMedicationService> _mockAnimalMedication = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private VisitService _service = null!;

    [SetUp]
    public void Setup()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockAnimalMedication = new Mock<IAnimalMedicationService>();

        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new VisitService(
            _context,
            new VisitMapper(),
            new AnimalBriefMapper(),
            new UserBriefMapper(),
            _mockUserManager.Object,
            _mockAnimalMedication.Object);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();
    
    [Test]
    public async Task GeneratePdfReportAsync_WhenAssignedVetMismatch_ShouldThrowUnauthorizedAccessException()
    {
        var owner = new User
        {
            Id = "owner-1",
            UserName = "owner@test.local",
            Email = "owner@test.local",
            FirstName = "O",
            LastName = "W"
        };
        var assignedVet = new User
        {
            Id = "vet-assigned",
            UserName = "assigned@test.local",
            Email = "assigned@test.local",
            FirstName = "A",
            LastName = "V"
        };
        var otherVetId = "vet-other";
        var animal = new Animal
        {
            Name = "Pet",
            BodyWeight = 1f,
            Gender = Gender.Male,
            Owner = owner
        };

        await _context.Users.AddRangeAsync(owner, assignedVet);
        await _context.Users.AddAsync(new User
        {
            Id = otherVetId,
            UserName = "other@test.local",
            Email = "other@test.local",
            FirstName = "O",
            LastName = "V"
        });
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var visit = new Visit
        {
            Title = "Check",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = assignedVet.Id
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        var act = async () => await _service.GeneratePdfReportAsync(
            visit.Id,
            otherVetId,
            new List<string> { "Vet" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}