using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Models.Enums;

public enum VisitStatus
{
    [Display(Name = "Scheduled")]
    Scheduled,
    
    [Display(Name = "In Progress")]
    InProgress,
    
    [Display(Name = "Completed")]
    Completed,
    
    [Display(Name = "Cancelled")]
    Cancelled
}