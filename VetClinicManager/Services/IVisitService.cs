using VetClinicManager.DTOs.Shared;
using VetClinicManager.DTOs.Visits;

namespace VetClinicManager.Services;

public interface IVisitService
{
    // For Staff Index GET action
    Task<IEnumerable<VisitListVetRecDto>> GetVisitsForStaffAsync(string? vetId = null);
    // For Staff Details GET action
    Task<VisitDetailsVetRecDto?> GetDetailsForStaffAsync(int id);
    
    // For Owner Index GET action
    Task<IEnumerable<VisitListUserDto>> GetVisitsForOwnerAsync(string ownerId);
    // For Owner Details GET action
    Task<VisitDetailsUserDto?> GetDetailsForOwnerAsync(int id, string ownerId);
    
    // For Edit GET action
    Task<VisitEditDto?> GetForEditAsync(int id, string userId, bool isVet);
    // For Delete GET action
    Task<VisitDeleteDto?> GetForDeleteAsync(int id);
    
    // For Create POST action
    Task<int> CreateVisitAsync(VisitCreateDto createDto);
    // For Edit POST action
    Task<bool> UpdateVisitAsync(int id, VisitEditDto editDto, string userId, bool isVet);
    // For Delete POST action
    Task<bool> DeleteVisitAsync(int id);
    
    // For Create/Edit view select lists
    Task<IEnumerable<AnimalBriefDto>> GetAnimalsForSelectListAsync();
    Task<IEnumerable<UserBriefDto>> GetVetsForSelectListAsync();
}
