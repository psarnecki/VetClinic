using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public static partial class MedicationBriefMapper
{
    // Projects a Medication query to its brief DTO form.
    public static partial IQueryable<MedicationBriefDto> ProjectToDto(this IQueryable<Medication> q);
    
    // Maps a Medication entity to its brief DTO representation.
    public static partial MedicationBriefDto ToMedicationBriefDto(Medication medication);
}