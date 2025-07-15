using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.DTOs.Animals;

public class AnimalEditDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(25)]
    [Display(Name = "Microchip ID")]
    public string? MicrochipId { get; set; }     
    
    [MaxLength(100)]
    [Display(Name = "Species")]
    public string? Species { get; set; }
    
    [MaxLength(100)]
    [Display(Name = "Breed")]
    public string? Breed { get; set; }
    
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    
    [Range(0, 5000)]
    [Display(Name = "Weight (kg)")]
    public float BodyWeight { get; set; }
    
    [Display(Name = "Gender")]
    public Gender Gender { get; set; }
    
    [Display(Name = "Current Photo")]
    [DataType(DataType.ImageUrl)]
    public string? ImageUrl { get; set; }
    
    [Display(Name = "Change Photo")]
    public IFormFile? ImageFile { get; set; }
    
    [Display(Name = "Owner")]
    public string? OwnerId { get; set; }
    
    public SelectList? Owners { get; set; }
    public SelectList? Genders { get; set; }
}