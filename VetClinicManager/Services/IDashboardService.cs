using VetClinicManager.DTOs.Home;

namespace VetClinicManager.Services;

public interface IDashboardService
{
    // For Staff Dashboard GET action
    Task<DashboardDto> GetDashboardDataForStaffAsync(string? vetId = null);
    // For Client Dashboard GET action
    Task<DashboardDto> GetDashboardDataForClientAsync(string userId);
}