using Riok.Mapperly.Abstractions;
using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.Models;

namespace VetClinicManager.Areas.Admin.Mappers;

[Mapper]
public partial class MedicationMapper
{
    // Mapping from Medication to MedicationListDto - single entity object (Index)
    public partial MedicationListDto ToMedicationListDto(Medication medication);
    
    // Mapping from Medication to MedicationDetailsDto (Details GET)
    public partial MedicationDetailsDto ToMedicationDetailsDto(Medication medication);

    // Mapping from Medication to MedicationEditDto (Edit GET)
    public partial MedicationEditDto ToMedicationEditDto(Medication medication);

    // Mapping from Medication to MedicationDeleteDto (Delete GET)
    public partial MedicationDeleteDto ToMedicationDeleteDto(Medication medication);

    // Mapping from MedicationCreateDto to Medication (Create POST)
    public partial Medication ToMedication(MedicationCreateDto createDto);

    // Mapping from MedicationEditDto to existing Medication (Edit POST)
    [MapperIgnoreSource(nameof(MedicationEditDto.Id))]
    public partial void UpdateMedicationFromDto(MedicationEditDto editDto, Medication medication);
}