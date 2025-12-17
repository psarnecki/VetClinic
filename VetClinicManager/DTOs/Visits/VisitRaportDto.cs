using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitReportDto
{
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public VisitStatus Status { get; set; }
    
    public VisitPriority? Priority { get; set; }
    
    public AnimalBriefDto Animal { get; set; }
    
    public UserBriefDto? Owner { get; set; }
    
    public UserBriefDto? AssignedVet { get; set; }
    
    public List<VisitUpdateBriefDto> Updates { get; set; } = new();
}
