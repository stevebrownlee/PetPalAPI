using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PetPal.API.Models;

namespace PetPal.API.Data;

public class PetPalDbContext : IdentityDbContext<IdentityUser>
{
    public PetPalDbContext(DbContextOptions<PetPalDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<PetOwner> PetOwners { get; set; }
    public DbSet<HealthRecord> HealthRecords { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<Weight> Weights { get; set; }
    public DbSet<Veterinarian> Veterinarians { get; set; }
    public DbSet<FeedingSchedule> FeedingSchedules { get; set; }
    public DbSet<CareProvider> CareProviders { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all DateTime properties to use UTC time
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }

        // Configure PetOwner as a join table
        modelBuilder.Entity<PetOwner>()
            .HasKey(po => po.Id);

        modelBuilder.Entity<PetOwner>()
            .HasOne(po => po.Pet)
            .WithMany(p => p.Owners)
            .HasForeignKey(po => po.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PetOwner>()
            .HasOne(po => po.UserProfile)
            .WithMany(up => up.OwnedPets)
            .HasForeignKey(po => po.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure HealthRecord relationships
        modelBuilder.Entity<HealthRecord>()
            .HasOne(hr => hr.Pet)
            .WithMany(p => p.HealthRecords)
            .HasForeignKey(hr => hr.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HealthRecord>()
            .HasOne(hr => hr.Veterinarian)
            .WithMany()
            .HasForeignKey(hr => hr.VeterinarianId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Appointment relationships
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Pet)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Veterinarian)
            .WithMany()
            .HasForeignKey(a => a.VeterinarianId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Medication relationships
        modelBuilder.Entity<Medication>()
            .HasOne(m => m.Pet)
            .WithMany(p => p.Medications)
            .HasForeignKey(m => m.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Weight relationships
        modelBuilder.Entity<Weight>()
            .HasOne(w => w.Pet)
            .WithMany(p => p.WeightRecords)
            .HasForeignKey(w => w.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure FeedingSchedule relationships
        modelBuilder.Entity<FeedingSchedule>()
            .HasOne(fs => fs.Pet)
            .WithMany(p => p.FeedingSchedules)
            .HasForeignKey(fs => fs.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure NotificationSettings relationship
        modelBuilder.Entity<NotificationSettings>()
            .HasOne(ns => ns.UserProfile)
            .WithOne(up => up.NotificationSettings)
            .HasForeignKey<NotificationSettings>(ns => ns.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}