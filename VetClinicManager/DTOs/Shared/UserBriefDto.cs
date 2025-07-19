using System.ComponentModel.DataAnnotations;

namespace VetClinicManager.DTOs.Shared;

public class UserBriefDto
{
    public string Id { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}