using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Models;

namespace VetClinicManager.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Animal> Animals { get; set; }
    public DbSet<AnimalMedication> AnimalMedications { get; set; }
    public DbSet<HealthRecord> HealthRecords { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<VisitUpdate> VisitUpdates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Animal -> HealthRecord (1:1)
        modelBuilder.Entity<Animal>()
            .HasOne(a => a.HealthRecord)
            .WithOne(hr => hr.Animal)
            .HasForeignKey<HealthRecord>(hr => hr.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Animal -> Visits (1:N)
        modelBuilder.Entity<Animal>()
            .HasMany(a => a.Visits)
            .WithOne(v => v.Animal)
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Animal -> AnimalMedications (1:N)
        modelBuilder.Entity<Animal>()
            .HasMany(a => a.AnimalMedications)
            .WithOne(am => am.Animal)
            .HasForeignKey(am => am.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Animals (1:N)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Animals)
            .WithOne(a => a.Owner)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // User -> AssignedVisits (1:N)
        modelBuilder.Entity<User>()
            .HasMany(u => u.AssignedVisits)
            .WithOne(v => v.AssignedVet)
            .HasForeignKey(v => v.AssignedVetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Visit -> VisitUpdates (1:N)
        modelBuilder.Entity<Visit>()
            .HasMany(v => v.Updates)
            .WithOne(vu => vu.Visit)
            .HasForeignKey(vu => vu.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VisitUpdate>()
            .HasOne(vu => vu.UpdatedByVet)
            .WithMany()
            .HasForeignKey(vu => vu.UpdatedByVetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Medication -> AnimalMedications (1:N)
        modelBuilder.Entity<Medication>()
            .HasMany(m => m.AnimalMedications)
            .WithOne(am => am.Medication)
            .HasForeignKey(am => am.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}