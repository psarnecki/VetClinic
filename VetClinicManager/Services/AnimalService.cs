using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Data;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Services;

public class AnimalService : IAnimalService
{
    private readonly ApplicationDbContext _context;
    private readonly AnimalMapper _animalMapper;
    private readonly UserBriefMapper _userBriefMapper;
    private readonly IFileService _fileService;
    private readonly UserManager<User> _userManager;

    public AnimalService(ApplicationDbContext context, AnimalMapper animalMapper, UserBriefMapper userBriefMapper, IFileService fileService, UserManager<User> userManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _animalMapper = animalMapper ?? throw new ArgumentNullException(nameof(animalMapper));
        _userBriefMapper = userBriefMapper ?? throw new ArgumentNullException(nameof(userBriefMapper));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }
    
    // For Staff Index GET action
    public async Task<IEnumerable<AnimalListVetRecDto>> GetAnimalsForStaffAsync()
    {
        var animals = await _context.Animals
            .AsNoTracking()
            .Include(a => a.Owner)
            .Include(a => a.HealthRecord)
            .Include(a => a.Visits)
            .ToListAsync();
        
        return animals.Select(animal =>
        {
            var dto = _animalMapper.ToListVetRecDto(animal);
            dto.LastVisitDate = animal.Visits.Any() ? animal.Visits.Max(v => v.CreatedDate) : null;
            
            return dto;
        });
    }
    
    // For Staff Details GET action
    public async Task<AnimalDetailsVetRecDto?> GetAnimalDetailsForStaffAsync(int id)
    {
        var animal = await _context.Animals
            .AsNoTracking()
            .Include(a => a.Owner)
            .Include(a => a.HealthRecord)
            .Include(a => a.Visits)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (animal == null) return null;

        var dto = _animalMapper.ToAnimalDetailsVetRecDto(animal);
        dto.LastVisitDate = animal.Visits.Any() ? animal.Visits.Max(v => v.CreatedDate) : null;
    
        return dto;
    }

    // For Owner Index GET action
    public async Task<IEnumerable<AnimalListUserDto>> GetAnimalsForOwnerAsync(string ownerId)
    {
        var animals = await _context.Animals
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .Include(a => a.HealthRecord)
            .Include(a => a.Visits)
            .ToListAsync();

        return animals.Select(animal =>
        {
            var dto = _animalMapper.ToListUserDto(animal);
            dto.LastVisitDate = animal.Visits.Any() ? animal.Visits.Max(v => v.CreatedDate) : null;
            
            return dto;
        });
    }

    // For Owner Details GET action
    public async Task<AnimalDetailsUserDto?> GetAnimalDetailsForOwnerAsync(int id, string ownerId)
    {
        var animal = await _context.Animals
            .AsNoTracking()
            .Include(a => a.HealthRecord)
            .Include(a => a.Visits)
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

        if (animal == null) return null;
        
        var dto = _animalMapper.ToAnimalDetailsUserDto(animal);
        dto.LastVisitDate = animal.Visits.Any() ? animal.Visits.Max(v => v.CreatedDate) : null;
    
        return dto;
    }
    
    // For Edit GET action
    public async Task<AnimalEditDto?> GetAnimalForEditAsync(int id)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
        
        if (animal == null) return null;
        
        return _animalMapper.ToEditDto(animal);
    }
    
    // For Delete GET action
    public async Task<AnimalDeleteDto?> GetAnimalForDeleteAsync(int id)
    {
        var animal = await _context.Animals
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (animal == null) return null;
        
        return _animalMapper.ToDeleteDto(animal);
    }

    // For Create POST action
    public async Task<int> CreateAnimalAsync(AnimalCreateDto createAnimalDto)
    {
        var animal = _animalMapper.ToEntity(createAnimalDto);
        
        if (createAnimalDto.ImageFile != null)
        {
            animal.ImageUrl = await _fileService.SaveFileAsync(createAnimalDto.ImageFile, "uploads/animals");
        }
        
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();
        
        return animal.Id;
    }
    
    // For Edit POST action
    public async Task<bool> UpdateAnimalAsync(AnimalEditDto animalEditDto)
    {
        var animal = await _context.Animals.FindAsync(animalEditDto.Id);
        
        if (animal == null) return false;
        
        if (animalEditDto.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(animal.ImageUrl))
            {
                _fileService.DeleteFile(animal.ImageUrl);
            }
            animal.ImageUrl = await _fileService.SaveFileAsync(animalEditDto.ImageFile, "uploads/animals");
        }

        _animalMapper.UpdateFromDto(animalEditDto, animal);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    // For Delete POST action
    public async Task<bool> DeleteAnimalAsync(int id)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
        
        if (animal == null) return true;

        var hasVisits = await _context.Visits.AnyAsync(v => v.AnimalId == id);
        
        if(hasVisits) return false;
        
        if (!string.IsNullOrEmpty(animal.ImageUrl))
        {
            _fileService.DeleteFile(animal.ImageUrl);
        }
        
        _context.Animals.Remove(animal);
        var savedChanges = await _context.SaveChangesAsync();
        
        return savedChanges > 0;
    }
    
    // For Create/Edit view owner select list
    public async Task<IEnumerable<UserBriefDto>> GetOwnersForSelectListAsync()
    {
        var clients = await _userManager.GetUsersInRoleAsync("Client");
        
        return clients
            .OrderBy(c => c.LastName)
            .Select(c => _userBriefMapper.ToUserBriefDto(c));
    }
}