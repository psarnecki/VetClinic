using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.Prescriptions;

namespace VetClinicManager.DTOs.VisitUpdates;

public class VisitUpdateCreateDto
{
    [Required]
    public int VisitId { get; set; }
    
    public string? VisitTitle { get; set; }
    
    public string? AnimalName { get; set; }
    
    [Display(Name = "Clinical Notes")]
    public string? Notes { get; set; }
    
    [Display(Name = "Attachment (Image)")]
    public IFormFile? ImageFile { get; set; }
    
    public SelectList? Medications { get; set; }
    
    public List<PrescriptionCreateDto> Prescriptions { get; set; } = new();
}