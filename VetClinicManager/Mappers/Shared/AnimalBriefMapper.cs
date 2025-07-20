using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers.Shared;

[Mapper]
public partial class AnimalBriefMapper
{
    // Maps an Animal entity to its brief DTO representation
    public partial AnimalBriefDto ToAnimalBriefDto(Animal animal);
}