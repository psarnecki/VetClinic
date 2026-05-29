using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class AnimalServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IFileService> _mockFileService = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private AnimalService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockFileService = new Mock<IFileService>();

        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new AnimalService(
            _context,
            new AnimalMapper(),
            new UserBriefMapper(),
            _mockFileService.Object,
            _mockUserManager.Object);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetAnimalsForOwnerAsync_ShouldReturnOnlyAnimalsOwnedByUser()
    {
        var ownerA = CreateUser("owner-a", "Alice", "OwnerA");
        var ownerB = CreateUser("owner-b", "Bob", "OwnerB");
        var animalForA = new Animal
        {
            Name = "Rex",
            BodyWeight = 10f,
            Gender = Gender.Male,
            Owner = ownerA
        };
        var animalForB = new Animal
        {
            Name = "Felix",
            BodyWeight = 4f,
            Gender = Gender.Male,
            Owner = ownerB
        };

        await _context.Users.AddRangeAsync(ownerA, ownerB);
        await _context.Animals.AddRangeAsync(animalForA, animalForB);
        await _context.SaveChangesAsync();

        var result = (await _service.GetAnimalsForOwnerAsync(ownerA.Id)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Rex");
    }

    [Test]
    public async Task DeleteAnimalAsync_WhenAnimalHasVisits_ShouldReturnFalse()
    {
        var owner = CreateUser("owner1", "O", "W");
        var vet = CreateUser("vet1", "Dr", "V");
        var animal = new Animal
        {
            Name = "Pet",
            BodyWeight = 1f,
            Gender = Gender.Female,
            Owner = owner
        };

        await _context.Users.AddRangeAsync(owner, vet);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        _context.Visits.Add(new Visit
        {
            Title = "Visit",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = animal.Id,
            AssignedVetId = vet.Id
        });
        await _context.SaveChangesAsync();

        var deleted = await _service.DeleteAnimalAsync(animal.Id);

        deleted.Should().BeFalse();
        (await _context.Animals.AnyAsync(a => a.Id == animal.Id)).Should().BeTrue();
    }

    [Test]
    public async Task CreateAnimalAsync_WhenNoImageFile_ShouldPersist()
    {
        var owner = CreateUser("owner1", "John", "Smith");
        await _context.Users.AddAsync(owner);
        await _context.SaveChangesAsync();

        var dto = new AnimalCreateDto
        {
            Name = "Luna",
            BodyWeight = 3.2f,
            Gender = Gender.Female,
            OwnerId = owner.Id
        };

        var id = await _service.CreateAnimalAsync(dto);

        id.Should().BeGreaterThan(0);
        var entity = await _context.Animals.AsNoTracking().SingleAsync(a => a.Id == id);
        entity.Name.Should().Be("Luna");
        entity.OwnerId.Should().Be(owner.Id);

        _mockFileService.Verify(
            f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task GetAnimalDetailsForOwnerAsync_WhenOwnerIdWrong_ShouldReturnNull()
    {
        var ownerA = CreateUser("a", "A", "One");
        var ownerB = CreateUser("b", "B", "Two");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = ownerA };
        _context.Users.AddRange(ownerA, ownerB);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var dto = await _service.GetAnimalDetailsForOwnerAsync(animal.Id, ownerB.Id);

        dto.Should().BeNull();
    }

    [Test]
    public async Task RemoveAnimalImageAsync_WhenNoImageUrl_ShouldReturnFalse()
    {
        var owner = CreateUser("o", "O", "W");
        var animal = new Animal { Name = "Pet", BodyWeight = 1f, Gender = Gender.Male, Owner = owner, ImageUrl = null };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var ok = await _service.RemoveAnimalImageAsync(animal.Id);

        ok.Should().BeFalse();
        _mockFileService.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAnimalAsync_WhenNewImageReplacesExisting_ShouldDeleteOldFileBeforeSavingNew()
    {
        var owner = CreateUser("o", "O", "W");
        var animal = new Animal
        {
            Name = "Pet",
            BodyWeight = 1f,
            Gender = Gender.Male,
            Owner = owner,
            ImageUrl = "/uploads/animals/old.jpg"
        };
        _context.Users.Add(owner);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(10);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

        _mockFileService.Setup(f => f.SaveFileAsync(mockFile.Object, "uploads/animals"))
            .ReturnsAsync("/uploads/animals/new.jpg");

        var edit = new AnimalEditDto
        {
            Id = animal.Id,
            Name = animal.Name,
            BodyWeight = animal.BodyWeight,
            Gender = animal.Gender,
            ImageFile = mockFile.Object
        };

        var ok = await _service.UpdateAnimalAsync(edit);

        ok.Should().BeTrue();
        _mockFileService.Verify(f => f.DeleteFile("/uploads/animals/old.jpg"), Times.Once);
        _mockFileService.Verify(f => f.SaveFileAsync(mockFile.Object, "uploads/animals"), Times.Once);

        var reloaded = await _context.Animals.AsNoTracking().SingleAsync(x => x.Id == animal.Id);
        reloaded.ImageUrl.Should().Be("/uploads/animals/new.jpg");
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