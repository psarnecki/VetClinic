using FluentAssertions;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class HealthRecordMapperTests
{
    private readonly HealthRecordMapper _mapper = new();

    [Test]
    public void ToDetailsDto_ShouldFlattenAnimalNameAndMapScalarFields()
    {
        var animal = new Animal
        {
            Id = 8,
            Name = "Buddy",
            BodyWeight = 20f,
            Gender = Gender.Male
        };

        var entity = new HealthRecord
        {
            Id = 11,
            AnimalId = animal.Id,
            Animal = animal,
            IsSterilized = true,
            ChronicDiseases = "Diabetes",
            Allergies = "None"
        };

        var dto = _mapper.ToDetailsDto(entity);

        dto.Id.Should().Be(11);
        dto.AnimalId.Should().Be(8);
        dto.AnimalName.Should().Be("Buddy");
        dto.IsSterilized.Should().BeTrue();
        dto.ChronicDiseases.Should().Be("Diabetes");
        dto.Allergies.Should().Be("None");
    }

    [Test]
    public void ToEntity_WhenMappingFromCreateDto_ShouldMapHealthRecordFields()
    {
        var createDto = new HealthRecordCreateDto
        {
            AnimalId = 5,
            IsSterilized = true,
            ChronicDiseases = "Asthma",
            Vaccinations = "Rabies",
            LastVaccinationDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var entity = _mapper.ToEntity(createDto);

        entity.AnimalId.Should().Be(5);
        entity.IsSterilized.Should().BeTrue();
        entity.ChronicDiseases.Should().Be("Asthma");
        entity.Vaccinations.Should().Be("Rabies");
        entity.LastVaccinationDate.Should().Be(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public void UpdateFromDto_ShouldUpdateMedicalFieldsAndPreserveIdAndAnimalId()
    {
        var entity = new HealthRecord
        {
            Id = 11,
            AnimalId = 8,
            IsSterilized = false,
            Allergies = "Old allergy"
        };

        var editDto = new HealthRecordEditDto
        {
            Id = 999,
            AnimalId = 777,
            IsSterilized = true,
            Allergies = "Pollen"
        };

        _mapper.UpdateFromDto(editDto, entity);

        entity.IsSterilized.Should().BeTrue();
        entity.Allergies.Should().Be("Pollen");
        entity.Id.Should().Be(11);
        entity.AnimalId.Should().Be(8);
    }
}