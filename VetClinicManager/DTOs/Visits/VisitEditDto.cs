using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitEditDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(150)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Visit status is required.")] 
    [Display(Name = "Status")]
    public VisitStatus Status { get; set; }
    
    [Required(ErrorMessage = "Visit priority is required.")]
    [Display(Name = "Priority")]
    public VisitPriority Priority { get; set; }
    
    [Display(Name = "Animal")]
    public AnimalBriefDto? Animal { get; set; }
    
    [Display(Name = "Assigned Veterinarian")]
    public string? AssignedVetId { get; set; }
    
    public SelectList? Vets { get; set; }
    public SelectList? Statuses { get; set; }
    public SelectList? Priorities { get; set; }
}