namespace VetClinicManager.DTOs.Shared;

public class VisitUpdateBriefDto
{
    public int Id { get; set; }
    
    public DateTime UpdateDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string UpdatedByVetName { get; set; }
    
    public List<PrescriptionBriefDto> Prescriptions { get; set; } = new List<PrescriptionBriefDto>();
}