using FluentAssertions;
using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Models;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class MedicationMapperTests
{
    private readonly MedicationMapper _mapper = new();

    [Test]
    public void ToMedicationListDto_ShouldMapNameAndDescription()
    {
        var medication = new Medication
        {
            Id = 3,
            Name = "Ibuprofen",
            Description = "Painkiller"
        };

        var dto = _mapper.ToMedicationListDto(medication);

        dto.Id.Should().Be(3);
        dto.Name.Should().Be("Ibuprofen");
        dto.Description.Should().Be("Painkiller");
    }

    [Test]
    public void ToMedication_WhenMappingFromCreateDto_ShouldMapNameAndDescription()
    {
        var createDto = new MedicationCreateDto
        {
            Name = "Paracetamol",
            Description = "Antipyretic"
        };

        var entity = _mapper.ToMedication(createDto);

        entity.Name.Should().Be("Paracetamol");
        entity.Description.Should().Be("Antipyretic");
        entity.Id.Should().Be(0);
    }

    [Test]
    public void UpdateMedicationFromDto_ShouldUpdateFieldsAndPreserveId()
    {
        var entity = new Medication
        {
            Id = 10,
            Name = "Old name",
            Description = "Old description"
        };

        var editDto = new MedicationEditDto
        {
            Id = 999,
            Name = "New name",
            Description = "New description"
        };

        _mapper.UpdateMedicationFromDto(editDto, entity);

        entity.Name.Should().Be("New name");
        entity.Description.Should().Be("New description");
        entity.Id.Should().Be(10);
    }
}