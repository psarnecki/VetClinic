using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Shared;

public class VisitBriefDto
{
    public int Id { get; set; }
    
    public AnimalBriefDto Animal { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public VisitStatus Status { get; set; }
    
    public UserBriefDto? AssignedVet { get; set; }
}