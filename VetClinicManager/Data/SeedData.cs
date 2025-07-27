using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Data;

public class SeedData
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedData(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedAnimalsAsync();
        await SeedHealthRecordsAsync();
        await SeedMedicationsAsync();
        await SeedVisitsAsync();
        await SeedVisitUpdatesAsync();
        await SeedAnimalMedicationsAsync();
        await SeedPrescriptionsAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roleNames = new[] { "Admin", "Vet", "Receptionist", "Client" };

        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var users = new List<User>
        {
            new User
            {
                UserName = "admin@vet.com",
                Email = "admin@vet.com",
                FirstName = "Walter",
                LastName = "Penn",
                EmailConfirmed = true
            },
            new User
            {
                UserName = "vet@vet.com",
                Email = "vet@vet.com",
                FirstName = "Jessica",
                LastName = "Watson",
                Specialization = "Exotic animal veterinarian",
                EmailConfirmed = true
            },
            new User
            {
                UserName = "receptionist@vet.com",
                Email = "receptionist@vet.com",
                FirstName = "Mary",
                LastName = "Morgan",
                EmailConfirmed = true
            },
            new User
            {
                UserName = "client@vet.com",
                Email = "client@vet.com",
                FirstName = "Daniel",
                LastName = "Parker",
                EmailConfirmed = true
            },
            new User
            {
            UserName = "client2@vet.com",
            Email = "client2@vet.com",
            FirstName = "Jeremy",
            LastName = "Smith",
            EmailConfirmed = true
        }
        };

        var passwords = new[] { "Admin123!", "Vet123!", "Rec123!", "Client123!", "Client123!" };
        var roles = new[] { "Admin", "Vet", "Receptionist", "Client", "Client" };

        for (int i = 0; i < users.Count; i++)
        {
            var existingUser = await _userManager.FindByEmailAsync(users[i].Email);
            if (existingUser == null)
            {
                var result = await _userManager.CreateAsync(users[i], passwords[i]);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(users[i], roles[i]);
                }
            }
        }
    }

    private async Task SeedAnimalsAsync()
    {
        if (await _context.Animals.AnyAsync()) return;

        var client1 = await _userManager.FindByEmailAsync("client@vet.com");
        var client2 = await _userManager.FindByEmailAsync("client2@vet.com");


        var animals = new List<Animal>
        {
            new Animal
            {
                Name = "Cody",
                Species = "Dog",
                Breed = "Mixed",
                DateOfBirth = new DateTime(2018, 5, 10),
                BodyWeight = 18.5f, 
                Gender = Gender.Male,
                ImageUrl = "/uploads/animals/default-dog.png",
                OwnerId = client1.Id,
                MicrochipId = "123456789012345",
            },
            new Animal
            {
                Name = "Whiskers",
                Species = "Cat",
                Breed = "British Shorthair",
                DateOfBirth = new DateTime(2020, 2, 15),
                BodyWeight = 4.2f,
                Gender = Gender.Male,
                ImageUrl = "/uploads/animals/default-cat-1.png",
                OwnerId = client2.Id
            },
            new Animal
            {
                Name = "Daisy",
                Species = "Cat",
                Breed = "Siamese",
                DateOfBirth = new DateTime(2019, 8, 22),
                BodyWeight = 3.1f,
                Gender = Gender.Female,
                ImageUrl = "/uploads/animals/default-cat-2.png",
                OwnerId = client2.Id,
                MicrochipId = "123456789012345",
            }
        };

        await _context.Animals.AddRangeAsync(animals);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedHealthRecordsAsync()
    {
        if (await _context.HealthRecords.AnyAsync()) return;

        var animals = await _context.Animals.ToListAsync();

        var healthRecords = new List<HealthRecord>
        {
            new HealthRecord
            {
                AnimalId = animals[0].Id,
                IsSterilized = true,
                ChronicDiseases = "None",
                Allergies = "None",
                Vaccinations = "Rabies, Parvovirus",
                LastVaccinationDate = DateTime.Now.AddMonths(-3)
            },
            new HealthRecord
            {
                AnimalId = animals[1].Id,
                IsSterilized = false,
                ChronicDiseases = "Choroba nerek",
                Allergies = "Pollen",
                Vaccinations = "Rabies, Feline herpesvirus",
                LastVaccinationDate = DateTime.Now.AddMonths(-6)
            },
            new HealthRecord
            {
                AnimalId = animals[2].Id,
                IsSterilized = true,
                ChronicDiseases = "None",
                Allergies = "None",
                Vaccinations = "Rabies, Panleukopenia",
                LastVaccinationDate = DateTime.Now.AddMonths(-1)
            }
        };

        await _context.HealthRecords.AddRangeAsync(healthRecords);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedMedicationsAsync()
    {
        if (await _context.Medications.AnyAsync()) return;

        var medications = new List<Medication>
        {
            new Medication { Name = "Antibiotic XYZ" },
            new Medication { Name = "Painkiller ABC" },
            new Medication { Name = "Ear drops DEF" },
            new Medication { Name = "Antifungal shampoo GHI" },
            new Medication { Name = "Tick prevention JKL" }
        };

        await _context.Medications.AddRangeAsync(medications);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedVisitsAsync()
    {
        if (await _context.Visits.AnyAsync()) return;

        var animals = await _context.Animals.ToListAsync();
        var vets = await _userManager.GetUsersInRoleAsync("Vet");
        
        var vetUser = vets.FirstOrDefault();
        
        var vetUserId = vetUser?.Id;
        
        var visits = new List<Visit>
        {
            new Visit
            {
                Title = "Rabies vaccination",
                Description = "Routine vaccination.",
                CreatedDate = DateTime.Now.AddDays(-10),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal,
                AnimalId = animals[0].Id,
                AssignedVetId = vetUserId
            },
            new Visit
            {
                Title = "Health checkup",
                Description = "Routine checkup.",
                CreatedDate = DateTime.Now.AddDays(-5),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal, 
                AnimalId = animals[1].Id,
                AssignedVetId = vetUserId
            },
            new Visit
            {
                Title = "Ear infection treatment",
                Description = "Antibiotic administration and ear examination.",
                CreatedDate = DateTime.Now.AddDays(-2),
                Status = VisitStatus.InProgress,
                Priority = VisitPriority.Urgent, 
                AnimalId = animals[2].Id,
                AssignedVetId = vetUserId
            },
            new Visit
            {
                Title = "Cast removal from broken limb",
                Description = "Removal of unnecessary immobilization from injured paw.",
                CreatedDate = DateTime.Now.AddDays(4),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Urgent, 
                AnimalId = animals[2].Id,
                AssignedVetId = vetUserId
            },
            new Visit
            {
                Title = "Surgical operation",
                Description = "Removal of swallowed toy from stomach.",
                CreatedDate = DateTime.UtcNow,
                Status = VisitStatus.InProgress,
                Priority = VisitPriority.Critical, 
                AnimalId = animals[0].Id,
                AssignedVetId = vetUserId
            }
        };

        await _context.Visits.AddRangeAsync(visits);
        await _context.SaveChangesAsync();
    }

    private async Task SeedVisitUpdatesAsync()
    {
        if (await _context.VisitUpdates.AnyAsync()) return;

        var visits = await _context.Visits.ToListAsync();
        var vets = await _userManager.GetUsersInRoleAsync("Vet");
        
        var vetUser = vets.FirstOrDefault();
        
        var vetUserId = vetUser?.Id;

        var updates = new List<VisitUpdate>
        {
            new VisitUpdate
            {
                Notes = "Vaccination completed, pet in good condition",
                UpdateDate = DateTime.Now.AddDays(-9),
                ImageUrl = "/uploads/visit-updates/vaccine.png",
                VisitId = visits[0].Id,
                UpdatedByVetId = vetUserId
            },
            new VisitUpdate
            {
                Notes = "Checkup showed good health condition",
                UpdateDate = DateTime.Now.AddDays(-4),
                ImageUrl = "/uploads/visit-updates/checkup.png",
                VisitId = visits[1].Id,
                UpdatedByVetId = vetUserId
            },
            new VisitUpdate
            {
                Notes = "Antibiotic treatment started",
                UpdateDate = DateTime.Now.AddDays(-1),
                ImageUrl = "/uploads/visit-updates/ear-infection.png",
                VisitId = visits[2].Id,
                UpdatedByVetId = vetUserId
            }
        };

        await _context.VisitUpdates.AddRangeAsync(updates);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedAnimalMedicationsAsync()
    {
        if (await _context.AnimalMedications.AnyAsync()) return;
        
        var animals = await _context.Animals.ToListAsync(); 
        var medications = await _context.Medications.ToListAsync();
        var visits = await _context.Visits.ToListAsync();
        var updates = await _context.VisitUpdates.ToListAsync(); 

        var animalMedications = new List<AnimalMedication>
        {
            new AnimalMedication
            {
                AnimalId = animals[0].Id,
                MedicationId = medications[0].Id,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(-3),
            },
            new AnimalMedication
            {
                AnimalId = animals[2].Id,
                MedicationId = medications[2].Id,
                StartDate = DateTime.Now.AddDays(-2),
                EndDate = DateTime.Now.AddDays(5),
            }
        };

        await _context.AnimalMedications.AddRangeAsync(animalMedications);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedPrescriptionsAsync()
    {
        if (await _context.Prescriptions.AnyAsync()) return;

        var medications = await _context.Medications.ToListAsync();
        var visitUpdates = await _context.VisitUpdates.ToListAsync();

        var prescriptions = new List<Prescription>
        {
            new Prescription
            {
                MedicationId = medications[1].Id,
                VisitUpdateId = visitUpdates[0].Id,
                Dosage = "1 tablet, if necessary, for post-vaccination pain."
            },
            new Prescription
            {
                MedicationId = medications[2].Id,
                VisitUpdateId = visitUpdates[2].Id,
                Dosage = "2 drops into the affected ear, twice a day for 7 days."
            },
            new Prescription
            {
                MedicationId = medications[1].Id,
                VisitUpdateId = visitUpdates[2].Id,
                Dosage = "Half a tablet once a day for 3 days."
            }
        };

        await _context.Prescriptions.AddRangeAsync(prescriptions);
        await _context.SaveChangesAsync();
    }
}