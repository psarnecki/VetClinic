using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Home;
using VetClinicManager.Mappers;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly DashboardMapper _dashboardMapper;

    public DashboardService(ApplicationDbContext context, DashboardMapper dashboardMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dashboardMapper = dashboardMapper ?? throw new ArgumentNullException(nameof(dashboardMapper));
    }

    // For Staff Dashboard GET action
    public async Task<DashboardDto> GetDashboardDataForStaffAsync(string? vetId = null)
    {
        var today = DateTime.Today;
        var now = DateTime.Now;

        var totalAnimals = await _context.Animals.CountAsync();
        
        var visitsQuery = _context.Visits.AsQueryable();
        if (!string.IsNullOrEmpty(vetId))
        {
            visitsQuery = visitsQuery.Where(v => v.AssignedVetId == vetId);
        }
        
        var totalVisits = await visitsQuery.CountAsync();
        var todayVisits = await visitsQuery
            .CountAsync(v => v.ScheduledAt.Date == today);
        var upcomingVisits = await visitsQuery
            .CountAsync(v => v.ScheduledAt > now && v.Status != VisitStatus.Completed && v.Status != VisitStatus.Cancelled);
        
        var clientRoleId = await _context.Roles
            .Where(r => r.Name == "Client")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();
        
        var totalClients = clientRoleId != null
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == clientRoleId)
            : 0;
        
        var totalMedications = await _context.Medications.CountAsync();

        var recentVisits = await _dashboardMapper
            .ProjectToVisitBriefDto(
                visitsQuery
                    .AsNoTracking()
                    .OrderByDescending(v => v.ScheduledAt)
                    .Take(7)
            )
            .ToListAsync();

        var recentAnimalsDto = await _dashboardMapper
            .ProjectToAnimalDashboardDto(
                _context.Animals
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Take(5)
            )
            .ToListAsync();

        return new DashboardDto
        {
            TotalAnimals = totalAnimals,
            TotalVisits = totalVisits,
            TodayVisits = todayVisits,
            UpcomingVisits = upcomingVisits,
            TotalClients = totalClients,
            TotalMedications = totalMedications,
            RecentVisits = recentVisits,
            RecentAnimals = recentAnimalsDto
        };
    }

    // For Client Dashboard GET action
    public async Task<DashboardDto> GetDashboardDataForClientAsync(string userId)
    {
        var now = DateTime.Now;

        var myAnimals = await _context.Animals
            .CountAsync(a => a.OwnerId == userId);

        var animalIds = await _context.Animals
            .Where(a => a.OwnerId == userId)
            .Select(a => a.Id)
            .ToListAsync();

        var myUpcomingVisits = await _context.Visits
            .CountAsync(v => animalIds.Contains(v.AnimalId) && 
                           v.ScheduledAt > now && 
                           v.Status != VisitStatus.Completed && 
                           v.Status != VisitStatus.Cancelled);

        var myPastVisits = await _context.Visits
            .CountAsync(v => animalIds.Contains(v.AnimalId) && 
                           v.Status == VisitStatus.Completed);

        var recentVisits = await _dashboardMapper
            .ProjectToVisitBriefDto(
                _context.Visits
                    .AsNoTracking()
                    .Where(v => animalIds.Contains(v.AnimalId))
                    .OrderByDescending(v => v.ScheduledAt)
                    .Take(7)
            )
            .ToListAsync();

        var recentAnimals = await _dashboardMapper
            .ProjectToAnimalDashboardDto(
                _context.Animals
                    .AsNoTracking()
                    .Where(a => a.OwnerId == userId)
                    .OrderByDescending(a => a.Id)
                    .Take(5)
            )
            .ToListAsync();

        return new DashboardDto
        {
            MyAnimals = myAnimals,
            MyUpcomingVisits = myUpcomingVisits,
            MyPastVisits = myPastVisits,
            TotalAnimals = myAnimals,
            TotalVisits = myPastVisits + myUpcomingVisits,
            RecentVisits = recentVisits,
            RecentAnimals = recentAnimals
        };
    }
}