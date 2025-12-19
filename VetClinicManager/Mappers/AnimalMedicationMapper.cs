using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class AnimalMedicationMapper
{
    // Use nested mappers for composition
    [UseMapper]
    private readonly MedicationBriefMapper _medicationBriefMapper = new();
    
    // --- Mappings to DTOs ---
    
    // Maps an AnimalMedication query to DTOs for the list in HealthRecord details
    public partial IQueryable<AnimalMedicationListDto> ProjectToDto(IQueryable<AnimalMedication> q);

    // Maps AnimalMedication entity to a DTO for the list in HealthRecord details
    [MapProperty(nameof(@AnimalMedication.Medication), nameof(@AnimalMedicationListDto.MedicationName))]
    public partial AnimalMedicationListDto ToListDto(AnimalMedication entity);

    // Maps AnimalMedication entity to a DTO for the edit form
    [MapProperty(nameof(@AnimalMedication.Animal.Name), nameof(AnimalMedicationEditDto.AnimalName))]
    [MapProperty(nameof(@AnimalMedication.Medication.Name), nameof(AnimalMedicationEditDto.MedicationName))]
    [MapProperty(nameof(@AnimalMedication.Animal.HealthRecord.Id), nameof(AnimalMedicationEditDto.HealthRecordId))]
    public partial AnimalMedicationEditDto ToEditDto(AnimalMedication entity);

    // Maps AnimalMedication entity to a DTO for the delete confirmation view
    [MapProperty(nameof(@AnimalMedication.Animal.Name), nameof(AnimalMedicationDeleteDto.AnimalName))]
    [MapProperty(nameof(@AnimalMedication.Medication.Name), nameof(AnimalMedicationDeleteDto.MedicationName))]
    public partial AnimalMedicationDeleteDto ToDeleteDto(AnimalMedication entity);
    
    // --- Mappings from DTOs to Entity ---

    // Maps a DTO from the create form to a new AnimalMedication entity
    [MapperIgnoreTarget(nameof(AnimalMedication.Id))]
    [MapperIgnoreTarget(nameof(AnimalMedication.Animal))]
    [MapperIgnoreTarget(nameof(AnimalMedication.Medication))]
    public partial AnimalMedication ToEntity(AnimalMedicationCreateDto dto);

    // Updates an existing AnimalMedication entity from the edit form DTO
    [MapperIgnoreTarget(nameof(AnimalMedication.Id))]
    [MapperIgnoreTarget(nameof(AnimalMedication.AnimalId))]
    [MapperIgnoreTarget(nameof(AnimalMedication.Animal))]
    [MapperIgnoreTarget(nameof(AnimalMedication.Medication))]
    public partial void UpdateFromDto(AnimalMedicationEditDto dto, AnimalMedication entity);
}