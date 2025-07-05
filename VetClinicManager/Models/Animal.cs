using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Models;

public class Animal
{
    [Key] 
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(25)]
    public string? MicrochipId { get; set; } 
    
    [MaxLength(100)]
    public string? Species { get; set; }
    
    [MaxLength(100)]
    public string? Breed { get; set; }

    public DateTime? DateOfBirth { get; set; }
    
    [Range(0, 5000)]
    public float BodyWeight { get; set; }
    
    public Gender Gender { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public HealthRecord? HealthRecord { get; set; }
    
    [ForeignKey("Owner")]
    public string? OwnerId { get; set; }
    public User? Owner { get; set; }
    
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<AnimalMedication> AnimalMedications { get; set; } = new List<AnimalMedication>();
}