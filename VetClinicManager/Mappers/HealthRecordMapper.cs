using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class HealthRecordMapper
{
    // --- Mappings TO DTOs ---

    // Maps HealthRecord entity to a DTO for the details view
    [MapProperty(nameof(HealthRecord.Animal.Name), nameof(HealthRecordDetailsDto.AnimalName))]
    public partial HealthRecordDetailsDto ToDetailsDto(HealthRecord entity);

    // Maps HealthRecord entity to a DTO for the edit form
    [MapProperty(nameof(HealthRecord.Animal.Name), nameof(HealthRecordEditDto.AnimalName))]
    public partial HealthRecordEditDto ToEditDto(HealthRecord entity);
    
    // Maps HealthRecord entity to a DTO for the delete confirmation view
    [MapProperty(nameof(HealthRecord.Animal.Name), nameof(HealthRecordDeleteDto.AnimalName))]
    public partial HealthRecordDeleteDto ToDeleteDto(HealthRecord entity);
    
    // --- Mappings from DTOs to Entity ---

    // Maps a DTO from create form to a new HealthRecord entity
    public partial HealthRecord ToEntity(HealthRecordCreateDto dto);

    // Updates an existing HealthRecord entity from an edit form DTO
    [MapperIgnoreTarget(nameof(HealthRecord.Id))]
    [MapperIgnoreTarget(nameof(HealthRecord.AnimalId))]
    [MapperIgnoreTarget(nameof(HealthRecord.Animal))]
    public partial void UpdateFromDto(HealthRecordEditDto dto, HealthRecord entity);
}