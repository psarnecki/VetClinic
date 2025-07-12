using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Areas.Admin.DTOs.Medications;

public class MedicationDetailsDto
{
    public int Id { get; set; }
    
    [Display(Name = "Name")]
    public string? Name { get; set; }
}