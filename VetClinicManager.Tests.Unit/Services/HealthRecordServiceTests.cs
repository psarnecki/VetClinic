using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class HealthRecordServiceTests
{
    private ApplicationDbContext _context = null!;
    private HealthRecordService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new HealthRecordService(_context, new HealthRecordMapper(), new AnimalMedicationMapper());
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetDetailsAsync_WhenIdExists_ShouldReturnDtoWithAnimalName()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var hr = new HealthRecord { AnimalId = animal.Id, IsSterilized = false };
        _context.HealthRecords.Add(hr);
        await _context.SaveChangesAsync();

        var dto = await _service.GetDetailsAsync(hr.Id);

        dto.Should().NotBeNull();
        dto!.AnimalName.Should().Be("Buddy");
    }
    
    [Test]
    public async Task PrepareCreateDtoAsync_WhenAnimalAlreadyHasRecord_ShouldReturnNull()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        _context.HealthRecords.Add(new HealthRecord { AnimalId = animal.Id, IsSterilized = false });
        await _context.SaveChangesAsync();

        var dto = await _service.PrepareCreateDtoAsync(animal.Id);

        dto.Should().BeNull();
    }

    [Test]
    public async Task PrepareCreateDtoAsync_WhenAnimalUnknown_ShouldReturnNull()
    {
        var dto = await _service.PrepareCreateDtoAsync(99999);

        dto.Should().BeNull();
    }

    [Test]
    public async Task PrepareCreateDtoAsync_WhenAnimalHasNoRecord_ShouldReturnDtoWithAnimalData()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var dto = await _service.PrepareCreateDtoAsync(animal.Id);

        dto.Should().NotBeNull();
        dto!.AnimalId.Should().Be(animal.Id);
        dto.AnimalName.Should().Be("Buddy");
    }

    [Test]
    public async Task CreateHealthRecordAsync_ShouldPersistRecordAndReturnId()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var id = await _service.CreateHealthRecordAsync(new HealthRecordCreateDto
        {
            AnimalId = animal.Id,
            IsSterilized = true,
            Allergies = "Pollen"
        });

        id.Should().BeGreaterThan(0);
        var entity = await _context.HealthRecords.AsNoTracking().SingleAsync(h => h.Id == id);
        entity.IsSterilized.Should().BeTrue();
        entity.Allergies.Should().Be("Pollen");
    }

    [Test]
    public async Task UpdateHealthRecordAsync_WhenIdUnknown_ShouldReturnFalse()
    {
        var ok = await _service.UpdateHealthRecordAsync(new HealthRecordEditDto { Id = 99999, AnimalId = 1 });

        ok.Should().BeFalse();
    }

    [Test]
    public async Task UpdateHealthRecordAsync_WhenRecordExists_ShouldUpdateAndReturnTrue()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var hr = new HealthRecord { AnimalId = animal.Id, IsSterilized = false, Allergies = "None" };
        _context.HealthRecords.Add(hr);
        await _context.SaveChangesAsync();

        var ok = await _service.UpdateHealthRecordAsync(new HealthRecordEditDto
        {
            Id = hr.Id,
            AnimalId = animal.Id,
            IsSterilized = true,
            Allergies = "Dust"
        });

        ok.Should().BeTrue();
        var reloaded = await _context.HealthRecords.AsNoTracking().SingleAsync(h => h.Id == hr.Id);
        reloaded.IsSterilized.Should().BeTrue();
        reloaded.Allergies.Should().Be("Dust");
    }

    [Test]
    public async Task DeleteHealthRecordAsync_WhenIdUnknown_ShouldReturnTrueIdempotently()
    {
        var ok = await _service.DeleteHealthRecordAsync(99999);

        ok.Should().BeTrue();
    }

    [Test]
    public async Task DeleteHealthRecordAsync_WhenRecordExists_ShouldRemoveAndReturnTrue()
    {
        var owner = NewUser("o", "O", "W");
        var animal = new Animal { Name = "Buddy", BodyWeight = 2f, Gender = Gender.Male, Owner = owner };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var hr = new HealthRecord { AnimalId = animal.Id, IsSterilized = false };
        _context.HealthRecords.Add(hr);
        await _context.SaveChangesAsync();

        var ok = await _service.DeleteHealthRecordAsync(hr.Id);

        ok.Should().BeTrue();
        (await _context.HealthRecords.CountAsync()).Should().Be(0);
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