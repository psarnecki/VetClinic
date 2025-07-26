using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Mappers;

namespace VetClinicManager.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly HealthRecordMapper _healthRecordMapper;

    public HealthRecordService(ApplicationDbContext context, HealthRecordMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _healthRecordMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    // For Details GET action
    public async Task<HealthRecordDetailsDto?> GetDetailsAsync(int id)
    {
        var healthRecord = await _context.HealthRecords
            .AsNoTracking()
            .Include(hr => hr.Animal)
            .FirstOrDefaultAsync(hr => hr.Id == id);
            
        return healthRecord == null ? null : _healthRecordMapper.ToDetailsDto(healthRecord);
    }
    
    // For Edit GET action
    public async Task<HealthRecordEditDto?> GetForEditAsync(int id)
    {
        var healthRecord = await _context.HealthRecords
            .AsNoTracking()
            .Include(hr => hr.Animal)
            .FirstOrDefaultAsync(h => h.Id == id);

        return healthRecord == null ? null : _healthRecordMapper.ToEditDto(healthRecord);
    }
    
    // For Delete GET action
    public async Task<HealthRecordDeleteDto?> GetForDeleteAsync(int id)
    {
        var healthRecord = await _context.HealthRecords
            .AsNoTracking()
            .Include(hr => hr.Animal)
            .FirstOrDefaultAsync(h => h.Id == id);
        
        return healthRecord == null ? null : _healthRecordMapper.ToDeleteDto(healthRecord);
    }
    
    // For Create POST action
    public async Task<int> CreateHealthRecordAsync(HealthRecordCreateDto createDto)
    {
        var entity = _healthRecordMapper.ToEntity(createDto);
        _context.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity.Id;
    }
    
    // For Edit POST action
    public async Task<bool> UpdateHealthRecordAsync(HealthRecordEditDto editDto)
    {
        var entityInDb = await _context.HealthRecords.FindAsync(editDto.Id);
        
        if (entityInDb == null) return false;

        _healthRecordMapper.UpdateFromDto(editDto, entityInDb);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    // For Delete POST action
    public async Task<bool> DeleteHealthRecordAsync(int id)
    {
        var entity = await _context.HealthRecords.FindAsync(id);
        
        if (entity == null) return true; 

        _context.Remove(entity);
        var savedChanges = await _context.SaveChangesAsync();
        
        return savedChanges > 0;
    }
    
    // Gets data for the Create view and validates if the animal exists and has no record
    public async Task<HealthRecordCreateDto?> PrepareCreateDtoAsync(int animalId)
    {
        var animal = await _context.Animals
            .AsNoTracking()
            .Include(a => a.HealthRecord)
            .FirstOrDefaultAsync(a => a.Id == animalId);

        if (animal == null || animal.HealthRecord != null) return null;

        return new HealthRecordCreateDto { AnimalId = animal.Id, AnimalName = animal.Name };
    }
}