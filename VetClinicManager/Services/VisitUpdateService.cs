using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Mappers;

namespace VetClinicManager.Services;

public class VisitUpdateService : IVisitUpdateService
{
    private readonly ApplicationDbContext _context;
    private readonly VisitUpdateMapper _visitUpdateMapper;
    private readonly IFileService _fileService;

    public VisitUpdateService(ApplicationDbContext context, VisitUpdateMapper visitUpdateMapper, IFileService fileService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _visitUpdateMapper = visitUpdateMapper ?? throw new ArgumentNullException(nameof(visitUpdateMapper));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }
    
    // For Create GET action
    public async Task<VisitUpdateCreateDto?> GetForCreateAsync(int visitId)
    {
        var visit = await _context.Visits.AsNoTracking()
            .Include(v => v.Animal)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null) return null;
        
        return new VisitUpdateCreateDto
        {
            VisitId = visitId,
            VisitTitle = visit.Title,
            AnimalName = visit.Animal.Name,
            Medications = await GetMedicationsSelectListAsync()
        };
    }
    
    // For Edit GET action
    public async Task<VisitUpdateEditDto?> GetForEditAsync(int id, string vetId)
    {
        var visitUpdate = await _context.VisitUpdates
            .AsNoTracking()
            .Include(vu => vu.Visit).ThenInclude(v => v.Animal)
            .Include(vu => vu.Prescriptions).ThenInclude(p => p.Medication)
            .FirstOrDefaultAsync(vu => vu.Id == id);
        
        if (visitUpdate == null) return null;
        
        if (visitUpdate.UpdatedByVetId != vetId) throw new UnauthorizedAccessException();
        
        var dto = _visitUpdateMapper.ToEditDto(visitUpdate);
        dto.Medications = await GetMedicationsSelectListAsync();
        
        return dto;
    }
    
    // For Delete GET action
    public async Task<VisitUpdateDeleteDto?> GetForDeleteAsync(int id, string vetId)
    {
        var visitUpdate = await _context.VisitUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(vu => vu.Id == id);
        
        if (visitUpdate == null || visitUpdate.UpdatedByVetId != vetId) return null;
        
        return _visitUpdateMapper.ToDeleteDto(visitUpdate);
    }
    
    // For Create POST action
    public async Task<int> CreateVisitUpdateAsync(VisitUpdateCreateDto createDto, string vetId)
    {
        var visitUpdate = _visitUpdateMapper.ToEntity(createDto);
        visitUpdate.UpdatedByVetId = vetId;
        visitUpdate.UpdateDate = DateTime.UtcNow;
        
        if (createDto.ImageFile != null)
        {
            visitUpdate.ImageUrl = await _fileService.SaveFileAsync(createDto.ImageFile, "uploads/attachments");
        }
        
        _context.VisitUpdates.Add(visitUpdate);
        await _context.SaveChangesAsync();
        
        return visitUpdate.VisitId;
    }
    
    // For Edit POST action
    public async Task<int> UpdateVisitUpdateAsync(VisitUpdateEditDto editDto, string vetId)
    {
        var visitUpdateInDb = await _context.VisitUpdates
            .Include(vu => vu.Prescriptions)
            .FirstOrDefaultAsync(vu => vu.Id == editDto.Id);
            
        if (visitUpdateInDb == null) throw new KeyNotFoundException("Visit update not found.");
        if (visitUpdateInDb.UpdatedByVetId != vetId) throw new UnauthorizedAccessException();

        _visitUpdateMapper.UpdateFromDto(editDto, visitUpdateInDb);
        visitUpdateInDb.UpdateDate = DateTime.UtcNow;
        
        if (editDto.ImageFile != null)
        {
            _fileService.DeleteFile(visitUpdateInDb.ImageUrl);
            visitUpdateInDb.ImageUrl = await _fileService.SaveFileAsync(editDto.ImageFile, "uploads/attachments");
        }
        
        visitUpdateInDb.Prescriptions.Clear();

        foreach (var prescriptionDto in editDto.Prescriptions)
        {
            visitUpdateInDb.Prescriptions.Add(_visitUpdateMapper.ToPrescriptionEntity(prescriptionDto));
        }
        
        await _context.SaveChangesAsync();
        
        return visitUpdateInDb.VisitId;
    }
    
    // For Delete POST action
    public async Task<int> DeleteVisitUpdateAsync(int id, string vetId)
    {
        var visitUpdate = await _context.VisitUpdates
            .FirstOrDefaultAsync(vu => vu.Id == id);
            
        if (visitUpdate == null) throw new KeyNotFoundException("Visit update not found.");
        if (visitUpdate.UpdatedByVetId != vetId) throw new UnauthorizedAccessException();
        
        _fileService.DeleteFile(visitUpdate.ImageUrl);
        _context.VisitUpdates.Remove(visitUpdate); // Cascade Delete
        await _context.SaveChangesAsync();
        
        return visitUpdate.VisitId;
    }
    
    // For Medication select list
    public async Task<SelectList> GetMedicationsSelectListAsync()
    {
        var items = await _context.Medications
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
            .ToListAsync();
        
        return new SelectList(items, "Value", "Text");
    }
}