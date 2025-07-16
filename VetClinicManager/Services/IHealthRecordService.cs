using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Models;

namespace VetClinicManager.Services;

public interface IHealthRecordService
{
    // For Details GET action
    Task<HealthRecordDetailsDto?> GetDetailsAsync(int id);
    // For Edit GET action
    Task<HealthRecordEditDto?> GetForEditAsync(int id);
    // For Delete GET action
    Task<HealthRecordDeleteDto?> GetForDeleteAsync(int id);
    
    // For Create POST action
    Task<int> CreateAsync(HealthRecordCreateDto createDto);
    // For Edit POST action
    Task<bool> UpdateAsync(HealthRecordEditDto editDto);
    // For Delete POST action
    Task<bool> DeleteAsync(int id);
    
    // Prepare to create DTO with animal validation (checks existence and existing record)
    Task<HealthRecordCreateDto?> PrepareCreateDtoAsync(int animalId);
}