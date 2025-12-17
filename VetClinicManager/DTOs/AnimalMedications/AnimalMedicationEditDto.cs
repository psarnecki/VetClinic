using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VetClinicManager.DTOs.AnimalMedications;

public class AnimalMedicationEditDto
{
    [Required]
    public int Id { get; set; }
    
    public int AnimalId { get; set; }

    [Required(ErrorMessage = "A medication must be selected.")]
    [Display(Name = "Medication")]
    public int? MedicationId { get; set; }
    
    [Display(Name = "Patient")]
    public string? AnimalName { get; set; }
    
    [Display(Name = "Medication")]
    public string? MedicationName { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date (optional)")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    public int HealthRecordId { get; set; }
    
    public SelectList? Medications { get; set; } 
}