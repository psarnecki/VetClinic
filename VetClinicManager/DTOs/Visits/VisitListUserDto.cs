using System.ComponentModel.DataAnnotations;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Visits;

public class VisitListUserDto
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Creation Date")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedDate { get; set; }
    
    public VisitStatus Status { get; set; }
    
    public AnimalBriefDto Animal { get; set; }
    
    public UserBriefDto? AssignedVet { get; set; }
}