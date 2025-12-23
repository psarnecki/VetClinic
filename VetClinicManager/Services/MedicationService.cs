using Microsoft.EntityFrameworkCore;
using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Mappers.Shared;

namespace VetClinicManager.Services;

public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _context;
    private readonly MedicationMapper _medicationMapper;
    private readonly MedicationBriefMapper _medicationBriefMapper;

    public MedicationService(ApplicationDbContext context, MedicationMapper medicationMapper, MedicationBriefMapper medicationBriefMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _medicationMapper = medicationMapper ?? throw new ArgumentNullException(nameof(medicationMapper));
        _medicationBriefMapper = medicationBriefMapper ?? throw new ArgumentNullException(nameof(medicationBriefMapper));
    }

    // For Index GET action
    public async Task<List<MedicationListDto>> GetAllMedicationsAsync()
    {
        var medications = await _context.Medications
            .AsNoTracking()
            .ToListAsync();
        
        var medicationListDtos = medications
            .Select(medication => _medicationMapper.ToMedicationListDto(medication))
            .ToList();
        
        return medicationListDtos;
    }

    // For Details GET action
    public async Task<MedicationDetailsDto?> GetMedicationForDetailsAsync(int id)
    {
        var medication = await _context.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication == null) return null;
        
        var detailsDto = _medicationMapper.ToMedicationDetailsDto(medication);
        
        return detailsDto;
    }
    
    // For Edit GET action
    public async Task<MedicationEditDto?> GetMedicationForEditAsync(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        
        if (medication == null) return null;
        
        var editDto = _medicationMapper.ToMedicationEditDto(medication);
        
        return editDto;
    }
    
    // For Delete GET action
    public async Task<MedicationDeleteDto?> GetMedicationForDeleteAsync(int id)
    {
        var medication = await _context.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
         
        if (medication == null) return null;
         
        var deleteDto = _medicationMapper.ToMedicationDeleteDto(medication); 
         
        return deleteDto;
    }
    
    // For Create POST action
    public async Task<MedicationListDto> CreateMedicationAsync(MedicationCreateDto createDto)
    {
        var medication = _medicationMapper.ToMedication(createDto);
        _context.Add(medication);
        await _context.SaveChangesAsync();
        var resultDto = _medicationMapper.ToMedicationListDto(medication);
        
        return resultDto;
    }

    // For Edit POST action
    public async Task<bool> UpdateMedicationAsync(MedicationEditDto editDto)
    {
        var medication = await _context.Medications.FindAsync(editDto.Id);
        
        if (medication == null) return false;
        
        _medicationMapper.UpdateMedicationFromDto(editDto, medication);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    // For Delete POST action
    public async Task<bool> DeleteMedicationAsync(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.AnimalMedications)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (medication == null) return true;
        
        if (medication.AnimalMedications.Any() || medication.Prescriptions.Any()) return false;
        
        _context.Medications.Remove(medication); 
        var savedChanges = await _context.SaveChangesAsync();
        
        return savedChanges > 0;
    }
    
    // For Medication select list
    public async Task<IEnumerable<MedicationBriefDto>> GetMedicationsForSelectListAsync()
    {
        return await _medicationBriefMapper.ProjectToDto(
            _context.Medications
                .AsNoTracking()
                .OrderBy(m => m.Name)
        ).ToListAsync();
    }
}