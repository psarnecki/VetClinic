using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Prescriptions;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class VisitUpdateMapper
{
    // --- Mappings to DTOs ---

    // Maps VisitUpdate entity to a DTO for the edit form
    [MapProperty(nameof(@VisitUpdate.Visit.Animal.Name), nameof(VisitUpdateEditDto.AnimalName))]
    [MapProperty(nameof(VisitUpdate.ImageUrl), nameof(VisitUpdateEditDto.ExistingImageUrl))]
    public partial VisitUpdateEditDto ToEditDto(VisitUpdate entity);

    // Maps VisitUpdate entity to a DTO for the delete confirmation view
    public partial VisitUpdateDeleteDto ToDeleteDto(VisitUpdate entity);

    // --- Mappings from DTOs to Entity ---

    // Maps a DTO from create form to a new VisitUpdate entity
    [MapperIgnoreTarget(nameof(VisitUpdate.Visit))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdatedByVet))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdatedByVetId))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdateDate))]
    [MapperIgnoreTarget(nameof(VisitUpdate.ImageUrl))]
    public partial VisitUpdate ToEntity(VisitUpdateCreateDto dto);
    
    // Updates an existing VisitUpdate entity from an edit form DTO
    [MapperIgnoreTarget(nameof(VisitUpdate.Id))]
    [MapperIgnoreTarget(nameof(VisitUpdate.VisitId))]
    [MapperIgnoreTarget(nameof(VisitUpdate.Visit))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdatedByVetId))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdatedByVet))]
    [MapperIgnoreTarget(nameof(VisitUpdate.UpdateDate))]
    [MapperIgnoreTarget(nameof(VisitUpdate.ImageUrl))]
    [MapperIgnoreTarget(nameof(VisitUpdate.Prescriptions))]
    public partial void UpdateFromDto(VisitUpdateEditDto dto, VisitUpdate entity);

    // --- Prescription Mappings (Child Entities) ---
    
    // Maps PrescriptionCreateDto to a new Prescription entity
    public partial Prescription ToPrescriptionEntity(PrescriptionCreateDto dto);
 
    // Maps PrescriptionEditDto to a new Prescription entity
    [MapperIgnoreTarget(nameof(Prescription.Id))]
    public partial Prescription ToPrescriptionEntity(PrescriptionEditDto dto);
    
    // Maps Prescription entity to a PrescriptionEditDto for the edit form
    public partial PrescriptionEditDto ToPrescriptionEditDto(Prescription entity);
}