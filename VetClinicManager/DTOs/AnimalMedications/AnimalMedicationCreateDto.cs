using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VetClinicManager.DTOs.AnimalMedications;

public class AnimalMedicationCreateDto
{
    [Required]
    public int AnimalId { get; set; }
    
    public int HealthRecordId { get; set; } 

    [Required(ErrorMessage = "A medication must be selected.")]
    [Display(Name = "Medication")]
    public int? MedicationId { get; set; }
    
    [Display(Name = "Patient")]
    public string? AnimalName { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "End Date (optional)")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    public SelectList? Medications { get; set; }
}