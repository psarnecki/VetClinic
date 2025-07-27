using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinicManager.Models;

public class Prescription
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Dosage is required.")]
    [MaxLength(250)]
    public string Dosage { get; set; }
    
    [Required]
    [ForeignKey("Medication")]
    public int MedicationId { get; set; }
    public Medication Medication { get; set; }
    
    [Required]
    [ForeignKey("VisitUpdate")]
    public int VisitUpdateId { get; set; }
    public VisitUpdate VisitUpdate { get; set; }
}