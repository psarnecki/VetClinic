using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.DTOs.Shared;

namespace VetClinicManager.Services;

public interface IMedicationService
{
    // For Index GET action
    Task<List<MedicationListDto>> GetAllMedicationsAsync();

    // For Details GET action
    Task<MedicationDetailsDto?> GetMedicationForDetailsAsync(int id);
    // For Edit GET action
    Task<MedicationEditDto?> GetMedicationForEditAsync(int id);
    // For Delete GET action
    Task<MedicationDeleteDto?> GetMedicationForDeleteAsync(int id);

    // For Create POST action
    Task<MedicationListDto> CreateMedicationAsync(MedicationCreateDto createDto);
    // For Edit POST action
    Task<bool> UpdateMedicationAsync(MedicationEditDto editDto);
    // For Delete POST action
    Task<bool> DeleteMedicationAsync(int id);
    
    // For Create/Edit view medications select list
    Task<IEnumerable<MedicationBriefDto>> GetMedicationsForSelectListAsync();
}