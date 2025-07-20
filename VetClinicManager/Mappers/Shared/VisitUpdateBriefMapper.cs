using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public partial class VisitUpdateBriefMapper
{
    // Use PrescriptionBriefMapper for nested mappings
    [UseMapper]
    private readonly PrescriptionBriefMapper _prescriptionMapper = new();
    
    // Maps a VisitUpdate entity to its brief DTO representation
    [MapProperty(nameof(VisitUpdate.UpdatedByVet), nameof(VisitUpdateBriefDto.UpdatedByVetName))]
    public partial VisitUpdateBriefDto ToVisitUpdateBriefDto(VisitUpdate visitUpdate);
    
    // Custom mapping method to convert a User object into a formatted string
    private string MapVetName(User vet)
    {
        return $"{vet.FirstName} {vet.LastName}".Trim();
    }
}