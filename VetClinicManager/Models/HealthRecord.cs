﻿using System.ComponentModel.DataAnnotations;
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
    
    [Display(Name = "Sterilized/Castrated")]
    public bool IsSterilized { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Chronic Diseases")]
    public string? ChronicDiseases { get; set; }
    
    [MaxLength(500)]
    public string? Allergies { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Vaccinations")]
    public string? Vaccinations { get; set; }
    
    [Display(Name = "Last Vaccination Date")]
    [DataType(DataType.Date)]
    public DateTime? LastVaccinationDate { get; set; }
}