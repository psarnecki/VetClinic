using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.Areas.Admin.DTOs.Users;

public class UserCreateDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(80)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(80)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [MaxLength(200)]
    [Display(Name = "Specialization")] 
    public string? Specialization { get; set; }
    
    public List<string> AvailableRoles { get; set; } = new List<string>();

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new List<string>();
}