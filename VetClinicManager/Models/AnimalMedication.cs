using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinicManager.Models;

public class AnimalMedication
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Animal")]
    public int AnimalId { get; set; } 
    public Animal Animal { get; set; }
    
    [Required]
    [ForeignKey("Medication")]
    public int MedicationId { get; set; }
    public Medication Medication { get; set; }
    
    [ForeignKey("Prescription")]
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}