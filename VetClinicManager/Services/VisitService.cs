using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Mappers;
using VetClinicManager.Models;

namespace VetClinicManager.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly VisitMapper _visitMapper;
    private readonly UserManager<User> _userManager;
    
    public VisitService(ApplicationDbContext context, VisitMapper visitMapper, UserManager<User> userManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _visitMapper = visitMapper ?? throw new ArgumentNullException(nameof(visitMapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }
    
    private IQueryable<Visit> GetBaseListQuery()
    {
        return _context.Visits.AsNoTracking()
            .Include(v => v.Animal).ThenInclude(a => a.Owner)
            .Include(v => v.AssignedVet);
    }
    
    private IQueryable<Visit> GetBaseDetailsQuery()
    {
        return GetBaseListQuery()
            .Include(v => v.Updates).ThenInclude(u => u.UpdatedByVet)
            .Include(v => v.Updates).ThenInclude(u => u.Prescriptions).ThenInclude(p => p.Medication);
    }
    
    // For Staff Index GET action
    public async Task<IEnumerable<VisitListVetRecDto>> GetVisitsForStaffAsync(string? vetId = null)
    {
        var visitsQuery = GetBaseListQuery();

        if (!string.IsNullOrEmpty(vetId))
        {
            visitsQuery = visitsQuery.Where(v => v.AssignedVetId == vetId);
        }
    
        var visits = await visitsQuery
            .OrderByDescending(v => v.CreatedDate)
            .ToListAsync();
            
        return _visitMapper.ToListVetRecDtos(visits);
    }

    // For Staff Details GET action
    public async Task<VisitDetailsVetRecDto?> GetDetailsForStaffAsync(int id)
    {
        var visit = await GetBaseDetailsQuery().FirstOrDefaultAsync(v => v.Id == id);
        
        return _visitMapper.ToDetailsVetRecDto(visit);
    }

    // For Owner Index GET action
    public async Task<IEnumerable<VisitListUserDto>> GetVisitsForOwnerAsync(string ownerId)
    {
        var visits = await GetBaseListQuery()
            .Where(v => v.Animal.OwnerId == ownerId)
            .OrderByDescending(v => v.CreatedDate)
            .ToListAsync();
            
        return _visitMapper.ToListUserDtos(visits);
    }

    // For Owner Details GET action
    public async Task<VisitDetailsUserDto?> GetDetailsForOwnerAsync(int id, string ownerId)
    {
        var visit = await GetBaseDetailsQuery().FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null || visit.Animal.OwnerId != ownerId) return null;
        
        return _visitMapper.ToDetailsUserDto(visit);
    }
    
    // For Edit GET action
    public async Task<VisitEditDto?> GetForEditAsync(int id, string userId, bool isVet)
    {
        var visit = await GetBaseListQuery().FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return null;
        
        if (isVet && visit.AssignedVetId != userId) throw new UnauthorizedAccessException();
        
        return _visitMapper.ToEditDto(visit);
    }
    
    // For Delete GET action
    public async Task<VisitDeleteDto?> GetForDeleteAsync(int id)
    {
        var visit = await GetBaseListQuery().FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return null;
        
        return _visitMapper.ToDeleteDto(visit);
    }
    
    // For Create POST action
    public async Task<int> CreateVisitAsync(VisitCreateDto createDto)
    {
        var visit = _visitMapper.ToEntity(createDto);
        visit.CreatedDate = DateTime.UtcNow;

        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();
        
        return visit.Id;
    }

    // For Edit POST action
    public async Task<bool> UpdateVisitAsync(int id, VisitEditDto editDto, string userId, bool isVet)
    {
        var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return false;
        
        if (isVet && visit.AssignedVetId != userId) throw new UnauthorizedAccessException();
        
        _visitMapper.UpdateFromDto(editDto, visit);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // For Delete POST action
    public async Task<bool> DeleteVisitAsync(int id)
    {
        var visit = await _context.Visits.FindAsync(id);
        
        if (visit == null) return true;

        _context.Visits.Remove(visit);
        var savedChanges = await _context.SaveChangesAsync();

        return savedChanges > 0;
    }

    // For Animal select list
    public async Task<IEnumerable<AnimalBriefDto>> GetAnimalsForSelectListAsync()
    {
        return await _context.Animals
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AnimalBriefDto 
            {
                Id = a.Id,
                Name = a.Name,
                Species = a.Species
            }).ToListAsync();
    }

    // For Vet select list
    public async Task<IEnumerable<UserBriefDto>> GetVetsForSelectListAsync()
    {
        var vets = await _userManager.GetUsersInRoleAsync("Vet");
        
        return vets
            .OrderBy(v => v.LastName)
            .Select(v => new UserBriefDto
            {
                Id = v.Id,
                FirstName = v.FirstName,
                LastName = v.LastName
            });
    }
}