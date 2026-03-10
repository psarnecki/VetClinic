using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitDeleteDto
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public VisitStatus Status { get; set; }
    
    public VisitPriority Priority { get; set; }

    public AnimalBriefDto Animal { get; set; }
    
    public UserBriefDto? Owner { get; set; }
    
    public UserBriefDto? AssignedVet { get; set; }
}