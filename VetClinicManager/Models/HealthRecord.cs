using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinicManager.Models;

public class HealthRecord
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Animal")]
    public int AnimalId { get; set; }
    public Animal Animal { get; set; }
    
    public bool IsSterilized { get; set; }
    
    [MaxLength(500)]
    public string? ChronicDiseases { get; set; }
    
    [MaxLength(500)]
    public string? Allergies { get; set; }
    
    [MaxLength(500)]
    public string? Vaccinations { get; set; }
    
    public DateTime? LastVaccinationDate { get; set; }
}