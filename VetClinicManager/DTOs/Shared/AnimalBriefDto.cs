namespace VetClinicManager.DTOs.Shared;

public class AnimalBriefDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Species { get; set; }
    
    public string? Breed { get; set; }
}