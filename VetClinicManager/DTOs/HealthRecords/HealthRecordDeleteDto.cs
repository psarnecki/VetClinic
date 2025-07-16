using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.HealthRecords;

public class HealthRecordDeleteDto
{
    public int Id { get; set; }
    
    [Display(Name = "Animal Name")]
    public string AnimalName { get; set; } = string.Empty; 
}