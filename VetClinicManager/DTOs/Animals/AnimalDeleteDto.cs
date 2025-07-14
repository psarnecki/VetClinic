using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.Animals;

public class AnimalDeleteDto
{
    public int Id { get; set; }
    
    [Display(Name = "Name")]
    public string? Name { get; set; }
    
    [Display(Name = "Species")]
    public string? Species { get; set; }

    [Display(Name = "Breed")]
    public string? Breed { get; set; }
    
    [Display(Name = "Microchip ID")]
    public string? MicrochipId { get; set; }
}