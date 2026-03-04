using VetClinicManager.DTOs.Shared;

namespace VetClinicManager.DTOs.Home;

public class DashboardDto
{
    // Common stats
    public int TotalAnimals { get; set; }
    public int TotalVisits { get; set; }
    
    // For Admin/Receptionist/Vet
    public int TodayVisits { get; set; }
    public int UpcomingVisits { get; set; }
    public int TotalClients { get; set; }
    public int TotalMedications { get; set; }
    
    // For Client
    public int MyAnimals { get; set; }
    public int MyUpcomingVisits { get; set; }
    public int MyPastVisits { get; set; }
    
    // Recent items
    public IEnumerable<VisitBriefDto> RecentVisits { get; set; } = new List<VisitBriefDto>();
    public IEnumerable<AnimalDashboardDto> RecentAnimals { get; set; } = new List<AnimalDashboardDto>();
}