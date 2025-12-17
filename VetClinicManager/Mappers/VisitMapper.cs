using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class VisitMapper
{
    // Use nested mappers for composition
    [UseMapper]
    private readonly UserBriefMapper _userBriefMapper = new();
    [UseMapper]
    private readonly AnimalBriefMapper _animalBriefMapper = new();
    [UseMapper]
    private readonly VisitUpdateBriefMapper _visitUpdateBriefMapper = new();
    
    // --- Mappings to DTOs ---
    
    // Maps Visit entity to a DTO for lists viewed by a client
    public partial VisitListUserDto ToListUserDto(Visit visit);
    
    // Maps Visit entity to a DTO for lists viewed by staff
    [MapProperty(nameof(Visit.Animal.Owner), nameof(VisitListVetRecDto.Owner))]
    public partial VisitListVetRecDto ToListVetRecDto(Visit visit);
    
    // Maps Visit entity to a DTO for details viewed by a client
    public partial VisitDetailsUserDto ToDetailsUserDto(Visit visit);
    
    // Maps Visit entity to a DTO for details viewed by staff
    [MapProperty(nameof(Visit.Animal.Owner), nameof(VisitDetailsVetRecDto.Owner))]
    public partial VisitDetailsVetRecDto ToDetailsVetRecDto(Visit visit);
    
    // Maps Visit entity to a DTO for the edit form
    public partial VisitEditDto ToEditDto(Visit visit);
    
    // Maps Visit entity to a DTO for the delete confirmation view
    [MapProperty(nameof(Visit.Animal.Owner), nameof(VisitDeleteDto.Owner))]
    public partial VisitDeleteDto ToDeleteDto(Visit visit);
    
    // --- Mappings from DTOs to Entity ---
    
    // Maps a DTO from create form to a new Visit entity
    public partial Visit ToEntity(VisitCreateDto dto);
    
    // Updates an existing Visit entity from an edit form DTO
    [MapperIgnoreTarget(nameof(Visit.Id))]
    [MapperIgnoreTarget(nameof(Visit.Animal))]
    [MapperIgnoreTarget(nameof(Visit.AnimalId))]
    public partial void UpdateFromDto(VisitEditDto dto, Visit visit);
    
    // --- Mappings for collections to DTOs ---
    
    // Maps a collection of Visit entities to a list of DTOs for clients
    public partial IEnumerable<VisitListUserDto> ToListUserDtos(IEnumerable<Visit> visits);
    
    // Maps a collection of Visit entities to a list of DTOs for staff
    public partial IEnumerable<VisitListVetRecDto> ToListVetRecDtos(IEnumerable<Visit> visits);
}