using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Models;

public class Visit
{
    [Key] 
    public int Id { get; set; }
  
    [Required]
    [MaxLength(150)]
    public string Title { get; set; }

    [MaxLength]
    public string? Description { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public VisitStatus Status { get; set; }
    
    public VisitPriority Priority { get; set; }

    [Required]
    [ForeignKey("Animal")]
    public int AnimalId { get; set; }
    public Animal Animal { get; set; }
    
    [ForeignKey("AssignedVet")]
    public string? AssignedVetId { get; set; }
    public User? AssignedVet { get; set; }

    public ICollection<VisitUpdate> Updates { get; set; } = new List<VisitUpdate>();
}