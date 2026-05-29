using FluentAssertions;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class AnimalMapperTests
{
    private readonly AnimalMapper _mapper = new();

    [Test]
    public void ToListVetRecDto_ShouldMapAnimalFieldsAndOwner()
    {
        var owner = new User
        {
            Id = "u1",
            UserName = "john.smith@test.local",
            Email = "john.smith@test.local",
            FirstName = "John",
            LastName = "Smith"
        };

        var animal = new Animal
        {
            Id = 42,
            Name = "Buddy",
            Species = "Dog",
            Breed = "Mixed breed",
            BodyWeight = 18.5f,
            Gender = Gender.Male,
            Owner = owner
        };

        AnimalListVetRecDto dto = _mapper.ToListVetRecDto(animal);

        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Buddy");
        dto.Species.Should().Be("Dog");
        dto.BodyWeight.Should().Be(18.5f);
        dto.Owner.Should().NotBeNull();
        dto.Owner!.FirstName.Should().Be("John");
        dto.Owner.LastName.Should().Be("Smith");
        dto.Owner.Email.Should().Be("john.smith@test.local");
    }

    [Test]
    public void ToEntity_WhenMappingFromCreateDto_ShouldMapAnimalFields()
    {
        var createDto = new AnimalCreateDto
        {
            Name = "Luna",
            Species = "Cat",
            BodyWeight = 3.2f,
            Gender = Gender.Female
        };

        Animal entity = _mapper.ToEntity(createDto);

        entity.Name.Should().Be("Luna");
        entity.Species.Should().Be("Cat");
        entity.BodyWeight.Should().Be(3.2f);
        entity.Gender.Should().Be(Gender.Female);
    }

    [Test]
    public void ToListUserDto_ShouldMapCoreAnimalFieldsAndFlattenHealthRecordId()
    {
        var animal = new Animal
        {
            Id = 5,
            Name = "Rex",
            Species = "Dog",
            BodyWeight = 12f,
            Gender = Gender.Male,
            OwnerId = "owner-1",
            Owner = new User
            {
                Id = "owner-1",
                UserName = "owner@test.local",
                Email = "owner@test.local"
            },
            HealthRecord = new HealthRecord { Id = 99 }
        };

        var dto = _mapper.ToListUserDto(animal);

        dto.Id.Should().Be(5);
        dto.Name.Should().Be("Rex");
        dto.Species.Should().Be("Dog");
        dto.HealthRecordId.Should().Be(99);
    }
}