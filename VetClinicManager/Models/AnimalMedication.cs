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
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    [ForeignKey("VisitUpdate")]
    public int? VisitUpdateId { get; set; }
    public VisitUpdate? VisitUpdate { get; set; }
}