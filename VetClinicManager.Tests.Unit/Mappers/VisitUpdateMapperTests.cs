using FluentAssertions;
using VetClinicManager.DTOs.Prescriptions;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class VisitUpdateMapperTests
{
    private readonly VisitUpdateMapper _mapper = new();

    [Test]
    public void ToEditDto_ShouldFlattenAnimalNameAndMapImageUrlToExistingImageUrl()
    {
        var animal = new Animal
        {
            Id = 3,
            Name = "Rex",
            BodyWeight = 12f,
            Gender = Gender.Male
        };

        var visit = new Visit
        {
            Id = 50,
            Title = "Follow-up",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.InProgress,
            Priority = VisitPriority.Normal,
            Animal = animal,
            AnimalId = animal.Id
        };

        var entity = new VisitUpdate
        {
            Id = 9,
            VisitId = visit.Id,
            Visit = visit,
            Notes = "Patient improving",
            ImageUrl = "/uploads/updates/img.jpg",
            UpdatedByVetId = "vet-1",
            UpdateDate = DateTime.UtcNow
        };

        var dto = _mapper.ToEditDto(entity);

        dto.Id.Should().Be(9);
        dto.VisitId.Should().Be(50);
        dto.Notes.Should().Be("Patient improving");
        dto.AnimalName.Should().Be("Rex");
        dto.ExistingImageUrl.Should().Be("/uploads/updates/img.jpg");
    }

    [Test]
    public void ToEntity_WhenMappingFromCreateDto_ShouldMapScalarsAndIgnoreServerControlledFields()
    {
        var createDto = new VisitUpdateCreateDto
        {
            VisitId = 77,
            Notes = "First note"
        };

        var entity = _mapper.ToEntity(createDto);

        entity.VisitId.Should().Be(77);
        entity.Notes.Should().Be("First note");
        entity.UpdatedByVetId.Should().BeNull();
        entity.ImageUrl.Should().BeNull();
    }

    [Test]
    public void UpdateFromDto_ShouldUpdateNotesAndPreserveIdAndVisitId()
    {
        var entity = new VisitUpdate
        {
            Id = 5,
            VisitId = 100,
            Notes = "Old note",
            UpdatedByVetId = "vet-1",
            UpdateDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var editDto = new VisitUpdateEditDto
        {
            Id = 999,
            VisitId = 888,
            Notes = "New note"
        };

        _mapper.UpdateFromDto(editDto, entity);

        entity.Notes.Should().Be("New note");
        entity.Id.Should().Be(5);
        entity.VisitId.Should().Be(100);
        entity.UpdatedByVetId.Should().Be("vet-1");
    }

    [Test]
    public void ToPrescriptionEntity_WhenMappingFromCreateDto_ShouldMapDosageAndMedicationId()
    {
        var createDto = new PrescriptionCreateDto
        {
            MedicationId = 4,
            Dosage = "10mg twice a day"
        };

        var entity = _mapper.ToPrescriptionEntity(createDto);

        entity.MedicationId.Should().Be(4);
        entity.Dosage.Should().Be("10mg twice a day");
    }
}