using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class DashboardServiceTests
{
    private ApplicationDbContext _context = null!;
    private DashboardService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new DashboardService(_context, new DashboardMapper());
    }

    [TearDown]
    public void TearDown() => _context.Dispose();
    
    [Test]
    public async Task GetDashboardDataForClientAsync_WhenUserHasNoAnimals_ShouldReturnZeroCountsAndEmptyLists()
    {
        var dto = await _service.GetDashboardDataForClientAsync("owner-with-no-pets");

        dto.MyAnimals.Should().Be(0);
        dto.MyUpcomingVisits.Should().Be(0);
        dto.MyPastVisits.Should().Be(0);
        dto.TotalVisits.Should().Be(0);
        dto.RecentVisits.Should().BeEmpty();
        dto.RecentAnimals.Should().BeEmpty();
    }

    [Test]
    public async Task GetDashboardDataForStaffAsync_WhenVetIdProvided_ShouldScopeVisitCountsToAssignedVetOnly()
    {
        var vet1 = NewUser("v1", "V", "One");
        var vet2 = NewUser("v2", "V", "Two");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        _context.Users.AddRange(vet1, vet2, owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var today = DateTime.Today;
        _context.Visits.Add(new Visit
        {
            Title = "A",
            ScheduledAt = today.AddHours(9),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet1.Id
        });
        _context.Visits.Add(new Visit
        {
            Title = "B",
            ScheduledAt = today.AddHours(10),
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet2.Id
        });
        await _context.SaveChangesAsync();

        var dto = await _service.GetDashboardDataForStaffAsync(vet1.Id);

        dto.TotalVisits.Should().Be(1);
        dto.TodayVisits.Should().Be(1);
    }

    [Test]
    public async Task GetDashboardDataForStaffAsync_WhenVetIdMissing_ShouldIncludeAllVisitsInTotal()
    {
        var vet1 = NewUser("v1", "V", "One");
        var vet2 = NewUser("v2", "V", "Two");
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "P", BodyWeight = 1f, Gender = Gender.Male, Owner = owner };
        _context.Users.AddRange(vet1, vet2, owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        _context.Visits.Add(new Visit
        {
            Title = "A",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet1.Id
        });
        _context.Visits.Add(new Visit
        {
            Title = "B",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet2.Id
        });
        await _context.SaveChangesAsync();

        var dto = await _service.GetDashboardDataForStaffAsync(vetId: null);

        dto.TotalVisits.Should().Be(2);
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