using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Models.Enums;

public enum Gender
{
    [Display(Name = "Unknown")]
    Unknown,
    
    [Display(Name = "Male")]
    Male,
    
    [Display(Name = "Female")]
    Female
}