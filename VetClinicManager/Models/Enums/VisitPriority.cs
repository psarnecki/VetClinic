using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Models.Enums;

public enum VisitPriority
{
    [Display(Name = "Normal")]
    Normal,
    
    [Display(Name = "Urgent")]
    Urgent,
    
    [Display(Name = "Critical")]
    Critical
}