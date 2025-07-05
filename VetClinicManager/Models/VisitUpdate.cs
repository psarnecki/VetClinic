using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinicManager.Models;

public class VisitUpdate
{
    [Key] 
    public int Id { get; set; }

    [MaxLength]
    public string? Notes { get; set; }
    
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [Required]
    [ForeignKey("Visit")]
    public int VisitId { get; set; }
    public Visit Visit { get; set; }

    [Required]
    [ForeignKey("UpdatedByVet")]
    public string UpdatedByVetId { get; set; }
    public User UpdatedByVet { get; set; } 
    
    public ICollection<AnimalMedication> AnimalMedications { get; set; } = new List<AnimalMedication>();
}