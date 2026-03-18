using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Services.Reports;

namespace VetClinicManager.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly VisitMapper _visitMapper;
    private readonly AnimalBriefMapper _animalBriefMapper;
    private readonly UserBriefMapper _userBriefMapper;
    private readonly UserManager<User> _userManager;
    private readonly IAnimalMedicationService _animalMedicationService;
    
    public VisitService(
        ApplicationDbContext context,
        VisitMapper visitMapper,
        AnimalBriefMapper animalBriefMapper,
        UserBriefMapper userBriefMapper,
        UserManager<User> userManager,
        IAnimalMedicationService animalMedicationService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _visitMapper = visitMapper ?? throw new ArgumentNullException(nameof(visitMapper));
        _animalBriefMapper = animalBriefMapper ?? throw new ArgumentNullException(nameof(animalBriefMapper));
        _userBriefMapper = userBriefMapper ?? throw new ArgumentNullException(nameof(userBriefMapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _animalMedicationService = animalMedicationService ?? throw new ArgumentNullException(nameof(animalMedicationService));
    }
    
    private IQueryable<Visit> GetBaseListQuery()
    {
        return _context.Visits.AsNoTracking()
            .Include(v => v.Animal)
                .ThenInclude(a => a.Owner)
            .Include(v => v.AssignedVet);
    }
    
    private IQueryable<Visit> GetBaseDetailsQuery()
    {
        return GetBaseListQuery()
            .Include(v => v.Updates)
                .ThenInclude(u => u.UpdatedByVet)
            .Include(v => v.Updates)
                .ThenInclude(u => u.Prescriptions)
                .ThenInclude(p => p.Medication);
    }
    
    // For Staff Index GET action
    public async Task<IEnumerable<VisitListVetRecDto>> GetVisitsForStaffAsync(string? vetId = null, string? sortOrder = null)
    {
        var visitsQuery = GetBaseListQuery();

        if (!string.IsNullOrEmpty(vetId))
        {
            visitsQuery = visitsQuery.Where(v => v.AssignedVetId == vetId);
        }
    
        var visits = await visitsQuery.ToListAsync();
        
        var visitDtos = _visitMapper.ToListVetRecDtos(visits);
        
        // Sorting
        var sortedVisits = sortOrder switch
        {
            "title" => visitDtos.OrderBy(v => v.Title),
            "title_desc" => visitDtos.OrderByDescending(v => v.Title),
            "scheduled" => visitDtos.OrderBy(v => v.ScheduledAt),
            "scheduled_desc" => visitDtos.OrderByDescending(v => v.ScheduledAt),
            "status" => visitDtos.OrderBy(v => v.Status),
            "status_desc" => visitDtos.OrderByDescending(v => v.Status),
            "priority" => visitDtos.OrderBy(v => v.Priority),
            "priority_desc" => visitDtos.OrderByDescending(v => v.Priority),
            "animal" => visitDtos.OrderBy(v => v.Animal.Name),
            "animal_desc" => visitDtos.OrderByDescending(v => v.Animal.Name),
            "owner" => visitDtos.OrderBy(v => v.Owner != null ? $"{v.Owner.FirstName} {v.Owner.LastName}" : ""),
            "owner_desc" => visitDtos.OrderByDescending(v => v.Owner != null ? $"{v.Owner.FirstName} {v.Owner.LastName}" : ""),
            _ => visitDtos.OrderByDescending(v => v.ScheduledAt) // Default
        };
        
        return sortedVisits.ToList();
    }

    // For Staff Details GET action
    public async Task<VisitDetailsVetRecDto?> GetDetailsForStaffAsync(int id)
    {
        var visit = await GetBaseDetailsQuery().FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return null;
        
        return _visitMapper.ToDetailsVetRecDto(visit);
    }

    // For Owner Index GET action
    public async Task<IEnumerable<VisitListUserDto>> GetVisitsForOwnerAsync(string ownerId, string? sortOrder = null)
    {
        var visits = await GetBaseListQuery()
            .Where(v => v.Animal.OwnerId == ownerId)
            .ToListAsync();
        
        var visitDtos = _visitMapper.ToListUserDtos(visits);
        
        // Sorting
        var sortedVisits = sortOrder switch
        {
            "title" => visitDtos.OrderBy(v => v.Title),
            "title_desc" => visitDtos.OrderByDescending(v => v.Title),
            "scheduled" => visitDtos.OrderBy(v => v.ScheduledAt),
            "scheduled_desc" => visitDtos.OrderByDescending(v => v.ScheduledAt),
            "status" => visitDtos.OrderBy(v => v.Status),
            "status_desc" => visitDtos.OrderByDescending(v => v.Status),
            "animal" => visitDtos.OrderBy(v => v.Animal.Name),
            "animal_desc" => visitDtos.OrderByDescending(v => v.Animal.Name),
            "vet" => visitDtos.OrderBy(v => v.AssignedVet != null ? $"{v.AssignedVet.FirstName} {v.AssignedVet.LastName}" : ""),
            "vet_desc" => visitDtos.OrderByDescending(v => v.AssignedVet != null ? $"{v.AssignedVet.FirstName} {v.AssignedVet.LastName}" : ""),
            _ => visitDtos.OrderByDescending(v => v.ScheduledAt) // Default
        };
        
        return sortedVisits.ToList();
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
        
        if (isVet && visit.AssignedVetId != userId) return null;
        
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
        visit.CreatedAt = DateTime.UtcNow;

        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();
        
        return visit.Id;
    }

    // For Edit POST action
    public async Task<bool> UpdateVisitAsync(int id, VisitEditDto editDto, string userId, bool isVet)
    {
        var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return false;
        
        if (isVet && visit.AssignedVetId != userId) return false;
        
        _visitMapper.UpdateFromDto(editDto, visit);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // For Delete POST action
    public async Task<bool> DeleteVisitAsync(int id)
    {
        var visit = await _context.Visits
            .Include(v => v.Updates)
                .ThenInclude(u => u.Prescriptions) 
            .FirstOrDefaultAsync(v => v.Id == id);
        
        if (visit == null) return true;
        
        foreach (var update in visit.Updates)
        {
            foreach (var prescription in update.Prescriptions)
            {
                await _animalMedicationService.SyncPrescriptionDeletedAsync(prescription.Id);
            }
        }

        _context.Visits.Remove(visit);
        var savedChanges = await _context.SaveChangesAsync();

        return savedChanges > 0;
    }

    // For Animal select list
    public async Task<IEnumerable<AnimalBriefDto>> GetAnimalsForSelectListAsync()
    {
        return await _animalBriefMapper.ProjectToDto(
            _context.Animals
                .AsNoTracking()
                .OrderBy(a => a.Name)
        ).ToListAsync();
    }

    // For Vet select list
    public async Task<IEnumerable<UserBriefDto>> GetVetsForSelectListAsync()
    {
        var vets = await _userManager.GetUsersInRoleAsync("Vet");
        
        return vets
            .OrderBy(v => v.LastName)
            .Select(v => _userBriefMapper.ToUserBriefDto(v));
    }
    
    public async Task<(byte[] FileContents, string FileName)?> GeneratePdfReportAsync(int visitId, string userId, IList<string> userRoles)
    {
        var visit = await GetBaseDetailsQuery().FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null) return null;

        bool isAdmin = userRoles.Contains("Admin");
        bool isVet = userRoles.Contains("Vet");
        bool isReceptionist = userRoles.Contains("Receptionist");
        
        if (isVet && !isAdmin && visit.AssignedVetId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        
        if (!isAdmin && !isVet && !isReceptionist && visit.Animal.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        var dto = _visitMapper.ToDetailsVetRecDto(visit);

        bool isStaffView = isAdmin || isVet || isReceptionist;

        var report = new VisitDetailsReport(dto, isStaffView);
        var bytes = report.GeneratePdf();
        
        string safeAnimalName = dto.Animal.Name.Replace(" ", "_");
        string dateString = dto.ScheduledAt.ToString("yyyy-MM-dd");
        string fileName = $"Visit_Report_{safeAnimalName}_{dateString}.pdf";
        
        return (bytes, fileName);
    }

    // For daily visit report background service
    public async Task<List<Visit>> GetOpenVisitsForReportAsync()
    {
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        return await GetBaseListQuery()
            .Where(v => (v.Status == Models.Enums.VisitStatus.Scheduled || v.Status == Models.Enums.VisitStatus.InProgress)
                     && v.ScheduledAt >= today && v.ScheduledAt < tomorrow)
            .OrderBy(v => v.ScheduledAt)
            .ToListAsync();
    }
}