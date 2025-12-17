using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.VisitUpdates;

public class VisitUpdateDeleteDto
{
    [Required]
    public int Id { get; set; }
    
    public int VisitId { get; set; }

    [Display(Name = "Update Date")]
    public DateTime UpdateDate { get; set; }

    [Display(Name = "Clinical Notes")]
    public string? Notes { get; set; }
}