using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public partial class AnimalBriefMapper
{
    // Projects an Animal query to its brief DTO form
    public partial IQueryable<AnimalBriefDto> ProjectToDto(IQueryable<Animal> q);
    
    // Maps an Animal entity to its brief DTO representation
    public partial AnimalBriefDto ToAnimalBriefDto(Animal animal);
}