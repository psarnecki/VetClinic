using VetClinicManager.DTOs.VisitUpdates;

namespace VetClinicManager.Services;

public interface IVisitUpdateService
{
    // For Create GET action
    Task<VisitUpdateCreateDto?> GetForCreateAsync(int visitId);
    // For Edit GET action
    Task<VisitUpdateEditDto?> GetForEditAsync(int id, string vetId, bool isAdmin);
    // For Delete GET action
    Task<VisitUpdateDeleteDto?> GetForDeleteAsync(int id, string vetId, bool isAdmin);

    // For Create POST action
    Task<int> CreateVisitUpdateAsync(VisitUpdateCreateDto createDto, string vetId);
    // For Edit POST action
    Task<int> UpdateVisitUpdateAsync(VisitUpdateEditDto editDto, string vetId, bool isAdmin);
    // For Delete POST action
    Task<int> DeleteVisitUpdateAsync(int id, string vetId, bool isAdmin);
}