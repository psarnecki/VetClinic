using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Areas.Admin.DTOs.Users;

public class UserEditDto
{
    [Required]
    public string Id { get; set; }
    
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(80)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(80)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Display(Name = "Specialization")]
    [MaxLength(200)]
    public string? Specialization { get; set; }

    public List<string> AvailableRoles { get; set; } = new List<string>();

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new List<string>();
}