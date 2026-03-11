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

    [MaxLength(200)]
    [Display(Name = "Specialization")]
    public string? Specialization { get; set; }

    public List<string> AvailableRoles { get; set; } = new List<string>();

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new List<string>();

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
}