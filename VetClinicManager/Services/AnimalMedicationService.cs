using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.Mappers;

namespace VetClinicManager.Services;

public class AnimalMedicationService : IAnimalMedicationService
{
    private readonly ApplicationDbContext _context;
    private readonly AnimalMedicationMapper _mapper;

    public AnimalMedicationService(ApplicationDbContext context, AnimalMedicationMapper animalMedicationMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = animalMedicationMapper ?? throw new ArgumentNullException(nameof(animalMedicationMapper));
    }

    // For Create GET action
    public async Task<AnimalMedicationCreateDto?> GetForCreateAsync(int animalId)
    {
        var animal = await _context.Animals
            .AsNoTracking()
            .Include(a => a.HealthRecord)
            .FirstOrDefaultAsync(a => a.Id == animalId);
        
        if (animal == null) return null;

        return new AnimalMedicationCreateDto
        {
            AnimalId = animalId,
            AnimalName = animal.Name,
            HealthRecordId = animal.HealthRecord?.Id ?? 0
        };
    }

    // For Edit GET action
    public async Task<AnimalMedicationEditDto?> GetForEditAsync(int id)
    {
        var animalMedication = await _context.AnimalMedications
            .AsNoTracking()
            .Include(am => am.Medication)
            .Include(am => am.Animal)
                .ThenInclude(a => a.HealthRecord)
            .FirstOrDefaultAsync(am => am.Id == id);
        
        if (animalMedication == null) return null;

        return _mapper.ToEditDto(animalMedication);
    }

    // For Delete GET action
    public async Task<AnimalMedicationDeleteDto?> GetForDeleteAsync(int id)
    {
        var animalMedication = await _context.AnimalMedications
            .AsNoTracking()
            .Include(am => am.Medication)
            .Include(am => am.Animal)
                .ThenInclude(a => a.HealthRecord)
            .FirstOrDefaultAsync(am => am.Id == id);
        
        if (animalMedication == null) return null;

        return _mapper.ToDeleteDto(animalMedication);
    }

    // For Create POST action
    public async Task<int> CreateAnimalMedicationAsync(AnimalMedicationCreateDto createDto)
    {
        var entity = _mapper.ToEntity(createDto);
        _context.AnimalMedications.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity.Id;
    }

    // For Edit POST action
    public async Task<bool> UpdateAnimalMedicationAsync(AnimalMedicationEditDto editDto)
    {
        var entityInDb = await _context.AnimalMedications.FindAsync(editDto.Id);
        
        if (entityInDb == null) return false;

        _mapper.UpdateFromDto(editDto, entityInDb);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // For Delete POST action
    public async Task<bool> DeleteAnimalMedicationAsync(int id)
    {
        var entity = await _context.AnimalMedications.FindAsync(id);
        
        if (entity == null) return true;

        _context.AnimalMedications.Remove(entity);
        var savedChanges = await _context.SaveChangesAsync();
        
        return savedChanges > 0;
    }
}