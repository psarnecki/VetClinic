using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.Models.Enums;

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
    
    // For Create POST action
    Task<int> CreateAnimalAsync(AnimalCreateDto animalCreateDto);
    // For Edit POST action
    Task<bool> UpdateAnimalAsync(AnimalEditDto animalEditDto);
    // For Delete POST action
    Task<bool> DeleteAnimalAsync(int id);
    
    // For Edit GET action
    Task<AnimalEditDto?> GetAnimalForEditAsync(int id);
    // For Delete GET action
    Task<AnimalEditDto?> GetAnimalForDeleteAsync(int id);

    // For Create/Edit view owner select list
    Task<SelectList> GetOwnersForSelectListAsync(string? selectedOwnerId = null);
    // For Create/Edit view gender select list
    SelectList GetGendersSelectList(Gender? selectedGender = null);
}