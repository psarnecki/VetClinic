namespace VetClinicManager.DTOs.Shared;

public class AnimalDashboardDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Species { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public UserBriefDto? Owner { get; set; }
}