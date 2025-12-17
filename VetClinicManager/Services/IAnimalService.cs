using VetClinicManager.DTOs.Animals;
using VetClinicManager.DTOs.Shared;

namespace VetClinicManager.Services;

public interface IAnimalService
{
    // For Staff Index GET action
    Task<IEnumerable<AnimalListVetRecDto>> GetAnimalsForStaffAsync();
    // For Staff Details GET action
    Task<AnimalDetailsVetRecDto?> GetAnimalDetailsForStaffAsync(int id);
    
    // For Owner Index GET action
    Task<IEnumerable<AnimalListUserDto>> GetAnimalsForOwnerAsync(string ownerId);
    // For Owner Details GET action
    Task<AnimalDetailsUserDto?> GetAnimalDetailsForOwnerAsync(int id, string ownerId);
    
    // For Edit GET action
    Task<AnimalEditDto?> GetAnimalForEditAsync(int id);
    // For Delete GET action
    Task<AnimalDeleteDto?> GetAnimalForDeleteAsync(int id);
    
    // For Create POST action
    Task<int> CreateAnimalAsync(AnimalCreateDto animalCreateDto);
    // For Edit POST action
    Task<bool> UpdateAnimalAsync(AnimalEditDto animalEditDto);
    // For Delete POST action
    Task<bool> DeleteAnimalAsync(int id);
    
    // For Create/Edit view select list
    Task<IEnumerable<UserBriefDto>> GetOwnersForSelectListAsync();
}