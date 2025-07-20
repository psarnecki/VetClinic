using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public partial class PrescriptionBriefMapper
{
    // Maps a Prescription entity to its brief DTO representation, including the Medication name
    [MapProperty(nameof(Prescription.Medication.Name), nameof(PrescriptionBriefDto.MedicationName))]
    public partial PrescriptionBriefDto ToPrescriptionBriefDto(Prescription prescription);
}