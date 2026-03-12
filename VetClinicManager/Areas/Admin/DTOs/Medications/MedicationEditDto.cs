using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Areas.Admin.DTOs.Medications;

public class MedicationEditDto
{
    [Required] 
    public int Id { get; set; }
    
    [Required(ErrorMessage = "The medication/material name is required.")]
    [MaxLength(150)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}