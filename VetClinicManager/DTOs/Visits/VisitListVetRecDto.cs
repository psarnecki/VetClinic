using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitListVetRecDto : VisitListUserDto
{
    public VisitPriority Priority { get; set; }
    
    public UserBriefDto? Owner { get; set; }
}