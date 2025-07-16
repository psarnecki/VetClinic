using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetClinicManager.Models;

public class User : IdentityUser
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(80)]
    public string LastName { get; set; }

    [MaxLength(200)]
    public string? Specialization { get; set; }
    
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
    public ICollection<Visit> AssignedVisits { get; set; } = new List<Visit>();
}