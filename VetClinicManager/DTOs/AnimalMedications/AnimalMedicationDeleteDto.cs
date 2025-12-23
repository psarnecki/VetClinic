using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.AnimalMedications;

public class AnimalMedicationDeleteDto
{
    public int Id { get; set; }
    
    public int HealthRecordId { get; set; }
    
    [Display(Name = "Patient")]
    public string AnimalName { get; set; } = string.Empty;
    
    [Display(Name = "Medication")]
    public string MedicationName { get; set; } = string.Empty;

    [Display(Name = "Start Date")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime StartDate { get; set; }
    
    [Display(Name = "End Date")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime? EndDate { get; set; } 
}