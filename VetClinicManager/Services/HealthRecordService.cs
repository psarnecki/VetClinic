using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Mappers;

namespace VetClinicManager.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly HealthRecordMapper _healthRecordMapper;
    private readonly AnimalMedicationMapper _animalMedicationMapper;

    public HealthRecordService(ApplicationDbContext context, HealthRecordMapper healthRecordMapper, AnimalMedicationMapper animalMedicationMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _healthRecordMapper = healthRecordMapper ?? throw new ArgumentNullException(nameof(healthRecordMapper));
        _animalMedicationMapper = animalMedicationMapper ?? throw new ArgumentNullException(nameof(animalMedicationMapper));
    }
    
    // For Details GET action
    public async Task<HealthRecordDetailsDto?> GetDetailsAsync(int id)
    {
        // KROK A: Pobierz Kartę Zdrowia (lekkie zapytanie)
        var healthRecord = await _context.HealthRecords
            .AsNoTracking()
            .Include(hr => hr.Animal) // Potrzebujemy tylko nazwy zwierzaka
            .FirstOrDefaultAsync(hr => hr.Id == id);
            
        if (healthRecord == null) return null;

        // Mapujemy główne DTO
        var dto = _healthRecordMapper.ToDetailsDto(healthRecord);

        // KROK B: Pobierz Leki (osobne, zoptymalizowane zapytanie)
        // Tworzymy zapytanie tylko do tabeli leków dla tego konkretnego zwierzaka
        var medicationsQuery = _context.AnimalMedications
            .AsNoTracking()
            .Where(am => am.AnimalId == healthRecord.AnimalId)
            .OrderByDescending(am => am.StartDate);

        // Używamy Mappera do projekcji (SELECT ... JOIN ... FROM Medications)
        // To wypełni listę DTO nazwami leków bez pobierania całej bazy
        dto.Medications = await _animalMedicationMapper.ProjectToDto(medicationsQuery).ToListAsync();

        return dto;
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