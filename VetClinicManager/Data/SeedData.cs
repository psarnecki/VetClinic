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

    public SeedData(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
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
                UserName = "vet2@vet.com",
                Email = "vet2@vet.com",
                FirstName = "Robert",
                LastName = "Hughes",
                Specialization = "Small animal veterinarian",
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

        var passwords = new[] { "Admin123!", "Vet123!", "Vet123!", "Rec123!", "Client123!", "Client123!" };
        var roles = new[] { "Admin", "Vet", "Vet", "Receptionist", "Client", "Client" };

        for (int i = 0; i < users.Count; i++)
        {
            var existingUser = await _userManager.FindByEmailAsync(users[i].Email!);
            
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
        
        if (client1 == null || client2 == null) return; 
        
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
                MicrochipId = "123456789012345"
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
                MicrochipId = "987654321098765"
            },
            new Animal
            {
                Name = "Rocky",
                Species = "Dog",
                Breed = "Labrador Retriever",
                DateOfBirth = new DateTime(2021, 3, 8),
                BodyWeight = 28.0f,
                Gender = Gender.Male,
                ImageUrl = "/uploads/animals/default-dog-2.png",
                OwnerId = client1.Id
            },
            new Animal
            {
                Name = "Luna",
                Species = "Rabbit",
                Breed = "Holland Lop",
                DateOfBirth = new DateTime(2022, 11, 14),
                BodyWeight = 1.8f,
                Gender = Gender.Female,
                ImageUrl = "/uploads/animals/default-rabbit.png",
                OwnerId = client2.Id
            },
            new Animal
            {
                Name = "Nibbles",
                Species = "Hamster",
                Breed = "Syrian",
                DateOfBirth = new DateTime(2023, 1, 5),
                BodyWeight = 0.15f,
                Gender = Gender.Male,
                OwnerId = client1.Id,
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
                ChronicDiseases = null,
                Allergies = null,
                Vaccinations = "Rabies, Parvovirus",
                LastVaccinationDate = DateTime.Now.AddMonths(-3)
            },
            new HealthRecord
            {
                AnimalId = animals[1].Id,
                IsSterilized = false,
                ChronicDiseases = "Kidney disease",
                Allergies = "Pollen",
                Vaccinations = "Rabies, Feline herpesvirus",
                LastVaccinationDate = DateTime.Now.AddMonths(-6)
            },
            new HealthRecord
            {
                AnimalId = animals[2].Id,
                IsSterilized = true,
                ChronicDiseases = null,
                Allergies = null,
                Vaccinations = "Rabies, Panleukopenia",
                LastVaccinationDate = DateTime.Now.AddMonths(-1)
            },
            new HealthRecord
            {
                AnimalId = animals[3].Id,
                IsSterilized = true,
                ChronicDiseases = null,
                Allergies = "Dust mites",
                Vaccinations = "Rabies, Distemper, Parvovirus",
                LastVaccinationDate = DateTime.Now.AddMonths(-2)
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
            new Medication
            {
                Name = "Antibiotic XYZ", 
                Description = "Broad-spectrum antibiotic used to treat bacterial infections. Administer as prescribed by the veterinarian."
            },
            new Medication
            {
                Name = "Painkiller ABC", 
                Description = "Non-steroidal anti-inflammatory drug (NSAID) for relief of mild to moderate pain and fever."
            },
            new Medication
            {
                Name = "Ear drops DEF", 
                Description = "Topical solution for treatment of bacterial and fungal ear infections. Apply directly into the ear canal."
            },
            new Medication
            {
                Name = "Antifungal shampoo GHI", 
                Description = "Medicated shampoo for treatment of fungal and bacterial skin conditions. Leave on for 5–10 minutes before rinsing."
            },
            new Medication
            {
                Name = "Tick prevention JKL", 
                Description = "Monthly topical treatment for prevention of ticks, fleas and mites. Apply to the back of the neck."
            },
            new Medication
            {
                Name = "Anti-inflammatory MNO", 
                Description = "Corticosteroid used to reduce inflammation and suppress immune response. Use only under veterinary supervision."
            },
            new Medication
            {
                Name = "Probiotic PQR", 
                Description = "Dietary supplement supporting healthy gut flora. Recommended after antibiotic therapy or during digestive issues."
            },
            new Medication
            {
                Name = "Deworming tablets STU", 
                Description = "Broad-spectrum antiparasitic tablets effective against roundworms, tapeworms and hookworms."
            }
        };

        await _context.Medications.AddRangeAsync(medications);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedVisitsAsync()
    {
        if (await _context.Visits.AnyAsync()) return;

        var animals = await _context.Animals.ToListAsync();
        var vets = await _userManager.GetUsersInRoleAsync("Vet");
        
        var vet1 = vets.FirstOrDefault(v => v.Email == "vet@vet.com");
        var vet2 = vets.FirstOrDefault(v => v.Email == "vet2@vet.com");
        
        var vet1Id = vet1?.Id;
        var vet2Id = vet2?.Id;
        
        var today = DateTime.Now.Date;
        
        var visits = new List<Visit>
        {
            // Jessica Watson visits
            new Visit
            {
                Title = "Rabies vaccination",
                Description = "Routine vaccination.",
                CreatedAt = DateTime.Now.AddDays(-8),
                ScheduledAt = DateTime.Now.AddDays(-10),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal,
                AnimalId = animals[0].Id,
                AssignedVetId = vet1Id
            },
            new Visit
            {
                Title = "Health checkup",
                Description = "Routine checkup.",
                CreatedAt = DateTime.Now.AddDays(-4),
                ScheduledAt = DateTime.Now.AddDays(-5),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal, 
                AnimalId = animals[1].Id,
                AssignedVetId = vet1Id
            },
            new Visit
            {
                Title = "Ear infection treatment",
                Description = "Antibiotic administration and ear examination.",
                CreatedAt = DateTime.Now.AddDays(-2),
                ScheduledAt = today.AddHours(DateTime.Now.Hour - 4),
                Status = VisitStatus.InProgress,
                Priority = VisitPriority.Urgent, 
                AnimalId = animals[2].Id,
                AssignedVetId = vet1Id
            },
            new Visit
            {
                Title = "Cast removal from broken limb",
                Description = "Removal of unnecessary immobilization from injured paw.",
                CreatedAt = DateTime.Now.AddDays(-1),
                ScheduledAt = DateTime.Now.AddDays(4).Date.AddHours(10),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Urgent, 
                AnimalId = animals[2].Id,
                AssignedVetId = vet1Id
            },
            new Visit
            {
                Title = "Surgical operation",
                Description = "Removal of swallowed toy from stomach.",
                CreatedAt = today.AddDays(-4),
                ScheduledAt = today.AddHours(DateTime.Now.Hour + 8),
                Status = VisitStatus.InProgress,
                Priority = VisitPriority.Critical, 
                AnimalId = animals[0].Id,
                AssignedVetId = vet1Id
            },
            
            // Robert Hughes visits
            new Visit
            {
                Title = "Vaccination booster",
                Description = "Scheduled booster dose for annual vaccination program.",
                CreatedAt = DateTime.Now.AddDays(-14),
                ScheduledAt = DateTime.Now.AddDays(-7).Date.AddHours(11),
                Status = VisitStatus.Cancelled,
                Priority = VisitPriority.Normal,
                AnimalId = animals[4].Id,
                AssignedVetId = vet2Id
            },
            new Visit
            {
                Title = "Post-surgery follow-up",
                Description = "Checking healing progress after neutering procedure.",
                CreatedAt = today.AddDays(-1),
                ScheduledAt = today.AddHours(9),
                Status = VisitStatus.Completed,
                Priority = VisitPriority.Normal,
                AnimalId = animals[3].Id,
                AssignedVetId = vet2Id
            },
            new Visit
            {
                Title = "Annual wellness exam",
                Description = "Comprehensive annual health examination and weight assessment.",
                CreatedAt = today.AddDays(-3),
                ScheduledAt = today.AddHours(DateTime.Now.Hour + 6),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                AnimalId = animals[4].Id,
                AssignedVetId = vet2Id
            },
            new Visit
            {
                Title = "Dental cleaning",
                Description = "Routine dental scaling and oral cavity inspection under sedation.",
                CreatedAt = DateTime.Now,
                ScheduledAt = today.AddDays(5),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                AnimalId = animals[3].Id,
                AssignedVetId = vet2Id
            },
            new Visit
            {
                Title = "First health examination",
                Description = "Initial examination of a new patient. Weight check, teeth and coat assessment.",
                CreatedAt = DateTime.Now,
                ScheduledAt = today.AddDays(3).AddHours(11),
                Status = VisitStatus.Scheduled,
                Priority = VisitPriority.Normal,
                AnimalId = animals[5].Id,
                AssignedVetId = vet1Id
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
        
        var vet1 = vets.FirstOrDefault(v => v.Email == "vet@vet.com");
        var vet2 = vets.FirstOrDefault(v => v.Email == "vet2@vet.com");
        
        if (vet1 == null || vet2 == null) return;

        var updates = new List<VisitUpdate>
        {
            new VisitUpdate
            {
                Notes = "Vaccination completed, pet in good condition",
                UpdateDate = DateTime.Now.AddDays(-9),
                ImageUrl = "/uploads/visit-updates/vaccine.png",
                VisitId = visits[0].Id,
                UpdatedByVetId = vet1.Id
            },
            new VisitUpdate
            {
                Notes = "Checkup showed good health condition",
                UpdateDate = DateTime.Now.AddDays(-4),
                ImageUrl = "/uploads/visit-updates/checkup.png",
                VisitId = visits[1].Id,
                UpdatedByVetId = vet1.Id
            },
            new VisitUpdate
            {
                Notes = "Antibiotic treatment started",
                UpdateDate = DateTime.Now.AddDays(-1),
                ImageUrl = "/uploads/visit-updates/ear-infection.png",
                VisitId = visits[2].Id,
                UpdatedByVetId = vet1.Id
            },
            new VisitUpdate
            {
                Notes = "Surgery in progress. Foreign object successfully located. Patient stable under anesthesia.",
                UpdateDate = DateTime.Now.Date.AddHours(8).AddMinutes(30),
                VisitId = visits[4].Id,
                UpdatedByVetId = vet1.Id
            },
            new VisitUpdate
            {
                Notes = "Follow-up completed. Healing progresses well, stitches removed. No complications observed.",
                UpdateDate = DateTime.Now.Date.AddHours(9).AddMinutes(30),
                VisitId = visits[6].Id,
                UpdatedByVetId = vet2.Id
            }
        };

        await _context.VisitUpdates.AddRangeAsync(updates);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedAnimalMedicationsAsync()
    {
        if (await _context.AnimalMedications.AnyAsync(am => am.PrescriptionId == null)) return;
        
        var animals = await _context.Animals.ToListAsync(); 
        var medications = await _context.Medications.ToListAsync();

        var animalMedications = new List<AnimalMedication>
        {
            new AnimalMedication
            {
                AnimalId = animals[0].Id,
                MedicationId = medications[0].Id,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(-3),
                PrescriptionId = null
            },
            new AnimalMedication
            {
                AnimalId = animals[2].Id,
                MedicationId = medications[2].Id,
                StartDate = DateTime.Now.AddDays(-2),
                EndDate = DateTime.Now.AddDays(5),
                PrescriptionId = null
            }
        };

        await _context.AnimalMedications.AddRangeAsync(animalMedications);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedPrescriptionsAsync()
    {
        if (await _context.Prescriptions.AnyAsync()) return;

        var animals = await _context.Animals.ToListAsync();
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
        
        var syncedMedications = new List<AnimalMedication>
        {
            new AnimalMedication
            {
                AnimalId = animals[0].Id,
                MedicationId = medications[1].Id,
                StartDate = visitUpdates[0].UpdateDate,
                PrescriptionId = prescriptions[0].Id,
                EndDate = null
            },
            new AnimalMedication
            {
                AnimalId = animals[2].Id,
                MedicationId = medications[2].Id,
                StartDate = visitUpdates[2].UpdateDate,
                PrescriptionId = prescriptions[1].Id,
                EndDate = null
            },
            new AnimalMedication
            {
                AnimalId = animals[2].Id,
                MedicationId = medications[1].Id,
                StartDate = visitUpdates[2].UpdateDate,
                PrescriptionId = prescriptions[2].Id,
                EndDate = null
            }
        };

        await _context.AnimalMedications.AddRangeAsync(syncedMedications);
        await _context.SaveChangesAsync();
    }
}