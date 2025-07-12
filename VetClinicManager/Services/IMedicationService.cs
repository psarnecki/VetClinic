using VetClinicManager.Areas.Admin.DTOs.Medications;

namespace VetClinicManager.Services;

public interface IMedicationService
{
    // For Index GET action
    Task<List<MedicationListDto>> GetAllMedicationsAsync();

    // For Details GET action
    Task<MedicationDetailsDto?> GetMedicationForDetailsAsync(int id);

    // For Create POST action
    Task<MedicationListDto> CreateMedicationAsync(MedicationCreateDto createDto);

    // For Edit GET action
    Task<MedicationEditDto?> GetMedicationForEditAsync(int id);

    // For Edit POST action
    Task<bool> UpdateMedicationAsync(MedicationEditDto editDto);

    // For Delete GET action
    Task<MedicationDeleteDto?> GetMedicationForDeleteAsync(int id);

    // For Delete POST action
    Task<bool> DeleteMedicationAsync(int id);
    
    // Helper method for checking if medication exists
    Task<bool> MedicationExistsAsync(int id);
}