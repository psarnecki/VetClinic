using System.ComponentModel.DataAnnotations;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Animals;

public class AnimalDetailsVetRecDto
{
    public int Id { get; set; }
    
    [Display(Name = "Name")]
    public string Name { get; set; }  = string.Empty;
    
    [Display(Name = "Microchip ID")]
    public string? MicrochipId { get; set; }
    
    [Display(Name = "Species")]
    public string? Species { get; set; }
    
    [Display(Name = "Breed")]
    public string? Breed { get; set; }
    
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    
    [Display(Name = "Weight (kg)")]
    public float BodyWeight { get; set; }
    
    [Display(Name = "Gender")]
    public Gender Gender { get; set; }
    
    [Display(Name = "Photo")]
    [DataType(DataType.ImageUrl)]
    public string? ImageUrl { get; set; }
    
    [Display(Name = "Last Visit")]
    [DataType(DataType.Date)]
    public DateTime? LastVisitDate { get; set; }
    
    [Display(Name = "Owner")]
    public UserBriefDto? Owner { get; set; }
    
    public int? HealthRecordId { get; set; }
    
    public bool HasHealthRecord => HealthRecordId.HasValue;
    public string HealthRecordButtonText => HasHealthRecord ? "View Health Record" : "Create Health Record";
    public string HealthRecordButtonClass => HasHealthRecord ? "btn-success" : "btn-secondary";
}