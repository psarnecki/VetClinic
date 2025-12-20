using VetClinicManager.DTOs.AnimalMedications;

namespace VetClinicManager.Services;

public interface IAnimalMedicationService
{
    // For Create GET action
    Task<AnimalMedicationCreateDto?> GetForCreateAsync(int animalId);
    // For Edit GET action
    Task<AnimalMedicationEditDto?> GetForEditAsync(int id);
    // For Delete GET action
    Task<AnimalMedicationDeleteDto?> GetForDeleteAsync(int id);

    // For Create POST action
    Task<int> CreateAnimalMedicationAsync(AnimalMedicationCreateDto createDto);
    // For Edit POST action
    Task<bool> UpdateAnimalMedicationAsync(AnimalMedicationEditDto editDto);
    // For Delete POST action
    Task<bool> DeleteAnimalMedicationAsync(int id);
}