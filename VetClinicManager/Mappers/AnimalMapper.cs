using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class AnimalMapper
{
    // Use UserBriefMapper for nested User -> UserBriefDto mappings
    [UseMapper]
    private readonly UserBriefMapper _userBriefMapper = new();
    
    // --- Mappings to DTOs ---
    
    // Maps Animal entity to a DTO for lists viewed by staff (includes Owner)
    [MapProperty(nameof(Animal.Owner), nameof(AnimalListVetRecDto.Owner))] 
    public partial AnimalListVetRecDto ToListVetRecDto(Animal animal);
    
    // Maps Animal entity to a DTO for lists viewed by a client (without Owner)
    public partial AnimalListUserDto ToListUserDto(Animal animal);
    
    // Maps Animal entity to a DTO for details viewed by staff (includes Owner)
    [MapProperty(nameof(Animal.Owner), nameof(AnimalDetailsVetRecDto.Owner))] 
    public partial AnimalDetailsVetRecDto ToAnimalDetailsVetRecDto(Animal animal);
    
    // Maps Animal entity to a DTO for details viewed by a client (without Owner)
    public partial AnimalDetailsUserDto ToAnimalDetailsUserDto(Animal animal);
    
    // Maps Animal entity to a DTO for the edit form
    public partial AnimalEditDto ToEditDto(Animal animal);
    
    // Maps Animal entity to a DTO for delete confirmation view
    public partial AnimalDeleteDto ToDeleteDto(Animal animal);
    
    // --- Mappings from DTOs to Entity ---
    
    // Maps a DTO from create form to a new Animal entity
    public partial Animal ToEntity(AnimalCreateDto dto);
    
    // Updates an existing Animal entity from an edit form DTO
    [MapperIgnoreTarget(nameof(Animal.Id))]
    [MapperIgnoreTarget(nameof(Animal.Owner))]
    [MapperIgnoreTarget(nameof(Animal.ImageUrl))]
    public partial void UpdateFromDto(AnimalEditDto dto, Animal animal);
}