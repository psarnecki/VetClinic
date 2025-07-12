using Microsoft.EntityFrameworkCore;
using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Data;

namespace VetClinicManager.Services;

public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _context;
    private readonly MedicationMapper _medicationMapper;

    public MedicationService(ApplicationDbContext context, MedicationMapper medicationMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _medicationMapper = medicationMapper ?? throw new ArgumentNullException(nameof(medicationMapper));
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
    public async Task<MedicationDeleteDto?> GetMedicationForDetailsAsync(int id)
    {
        var medication = await _context.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication == null) return null;
        var detailsDto = _medicationMapper.ToMedicationDeleteDto(medication);
        
        return detailsDto;
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

    // For Edit GET action
    public async Task<MedicationEditDto?> GetMedicationForEditAsync(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication == null) return null;
        var editDto = _medicationMapper.ToMedicationEditDto(medication);
        
        return editDto;
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

    // For Delete POST action
    public async Task<bool> DeleteMedicationAsync(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.AnimalMedications)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (medication == null) return true;
        
        if (medication.AnimalMedications.Any()) return false;
        
        _context.Medications.Remove(medication); 
        var savedChanges = await _context.SaveChangesAsync();
        
        return savedChanges > 0;
    }
    
    // For existence check helper
    public async Task<bool> MedicationExistsAsync(int id)
    {
        return await _context.Medications.AnyAsync(e => e.Id == id);
    }
}