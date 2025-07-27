using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitCreateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(150)]
    [Display(Name = "Title / Reason for Visit")]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Description / Details")]
    public string? Description { get; set; }
    
    [Display(Name = "Status")]
    public VisitStatus Status { get; set; }
    
    [Display(Name = "Priority")]
    public VisitPriority Priority { get; set; }
    
    [Required(ErrorMessage = "An animal must be selected.")]
    [Display(Name = "Animal")]
    public int AnimalId { get; set; }
    
    [Display(Name = "Assigned Veterinarian")]
    public string? AssignedVetId { get; set; }
    
    public SelectList? Animals { get; set; }
    public SelectList? Vets { get; set; }
    public SelectList? Statuses { get; set; }
    public SelectList? Priorities { get; set; }
}
