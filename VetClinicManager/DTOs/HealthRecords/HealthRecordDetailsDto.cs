using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.HealthRecords;

public class HealthRecordDetailsDto
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    
    public string AnimalName { get; set; } = string.Empty;

    [Display(Name = "Sterilized / Castrated")]
    public bool IsSterilized { get; set; }

    [Display(Name = "Chronic Diseases")]
    public string? ChronicDiseases { get; set; }

    [Display(Name = "Allergies")]
    public string? Allergies { get; set; }

    [Display(Name = "Vaccinations")]
    public string? Vaccinations { get; set; }

    [Display(Name = "Last Vaccination Date")]
    [DataType(DataType.Date)]
    public DateTime? LastVaccinationDate { get; set; }

    public string IsSterilizedText => IsSterilized ? "Yes" : "No";
    public string IsSterilizedBadgeClass => IsSterilized ? "bg-success" : "bg-danger";
}