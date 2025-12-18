using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public partial class MedicationBriefMapper
{
    // Projects a Medication query to its brief DTO form.
    public partial IQueryable<MedicationBriefDto> ProjectToDto(IQueryable<Medication> q);
    
    // Maps a Medication entity to its brief DTO representation.
    public partial MedicationBriefDto ToMedicationBriefDto(Medication medication);
}