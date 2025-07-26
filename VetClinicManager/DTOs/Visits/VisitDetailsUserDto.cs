using VetClinicManager.DTOs.Shared;

namespace VetClinicManager.DTOs.Visits;

public class VisitDetailsUserDto : VisitListUserDto
{
    public string? Description { get; set; }
    
    public List<VisitUpdateBriefDto> Updates { get; set; } = new();
}