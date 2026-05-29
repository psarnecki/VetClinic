using FluentAssertions;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class AnimalMedicationMapperTests
{
    private readonly AnimalMedicationMapper _mapper = new();

    [Test]
    public void ToEditDto_ShouldFlattenRelatedNamesAndHealthRecordId()
    {
        var animal = new Animal
        {
            Id = 4,
            Name = "Whiskers",
            BodyWeight = 5f,
            Gender = Gender.Female,
            HealthRecord = new HealthRecord { Id = 33 }
        };

        var medication = new Medication { Id = 7, Name = "Amoxicillin" };

        var entity = new AnimalMedication
        {
            Id = 12,
            AnimalId = animal.Id,
            Animal = animal,
            MedicationId = medication.Id,
            Medication = medication,
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.ToEditDto(entity);

        dto.Id.Should().Be(12);
        dto.MedicationId.Should().Be(7);
        dto.AnimalName.Should().Be("Whiskers");
        dto.MedicationName.Should().Be("Amoxicillin");
        dto.HealthRecordId.Should().Be(33);
        dto.StartDate.Should().Be(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        dto.EndDate.Should().Be(new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public void ToListDto_ShouldMapMedicationNavigationToBriefDto()
    {
        var medication = new Medication { Id = 7, Name = "Amoxicillin" };
        var entity = new AnimalMedication
        {
            Id = 1,
            AnimalId = 4,
            MedicationId = medication.Id,
            Medication = medication,
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.ToListDto(entity);

        dto.Id.Should().Be(1);
        dto.MedicationName.Should().NotBeNull();
        dto.MedicationName.Id.Should().Be(7);
        dto.MedicationName.Name.Should().Be("Amoxicillin");
    }

    [Test]
    public void ToEntity_WhenMappingFromCreateDto_ShouldMapScalarsAndIgnoreNavigations()
    {
        var createDto = new AnimalMedicationCreateDto
        {
            AnimalId = 4,
            MedicationId = 7,
            StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var entity = _mapper.ToEntity(createDto);

        entity.AnimalId.Should().Be(4);
        entity.MedicationId.Should().Be(7);
        entity.StartDate.Should().Be(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        entity.Id.Should().Be(0);
    }
}