using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.HealthRecords;

public class HealthRecordCreateDto
{
    [Required]
    public int AnimalId { get; set; } 
    
    public string AnimalName { get; set; } = string.Empty;

    [Display(Name = "Sterilized / Castrated")]
    public bool IsSterilized { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Chronic Diseases")]
    public string? ChronicDiseases { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Allergies")]
    public string? Allergies { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Vaccinations")]
    public string? Vaccinations { get; set; }
    
    [Display(Name = "Last Vaccination Date")]
    [DataType(DataType.Date)]
    public DateTime? LastVaccinationDate { get; set; }
}