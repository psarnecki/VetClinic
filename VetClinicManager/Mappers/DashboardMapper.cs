using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class DashboardMapper
{
    // Use nested mappers for composition
    [UseMapper]
    private readonly UserBriefMapper _userBriefMapper = new();
    [UseMapper]
    private readonly AnimalBriefMapper _animalBriefMapper = new();
    
    // --- IQueryable Projections for Dashboard ---
    
    // Projects a Visit query to VisitBriefDto
    public partial IQueryable<VisitBriefDto> ProjectToVisitBriefDto(IQueryable<Visit> query);
    
    // Projects an Animal query to AnimalDashboardDto
    public partial IQueryable<AnimalDashboardDto> ProjectToAnimalDashboardDto(IQueryable<Animal> query);
}