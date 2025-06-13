using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class VaccinationEndpoints
{
    public static void MapVaccinationEndpoints(this WebApplication app)
    {
        // Get all vaccinations for a pet
        app.MapGet("/pets/{petId}/vaccinations", async (
            int petId,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var pet = await db.Pets
                .Include(p => p.Owners)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Check if the user is an admin, vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            var vaccinations = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.PetId == petId && hr.RecordType == "Vaccination")
                .OrderByDescending(hr => hr.RecordDate)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<VaccinationDto>>(vaccinations));
        }).RequireAuthorization();

        // Get a specific vaccination
        app.MapGet("/vaccinations/{id}", async (
            int id,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var vaccination = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == id && hr.RecordType == "Vaccination");

            if (vaccination == null)
            {
                return Results.NotFound("Vaccination record not found.");
            }

            // Check if the user is an admin, vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = vaccination.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<VaccinationDto>(vaccination));
        }).RequireAuthorization();

        // Create a new vaccination
        app.MapPost("/vaccinations", async (
            [FromBody] VaccinationCreateDto vaccinationDto,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var pet = await db.Pets
                .Include(p => p.Owners)
                .FirstOrDefaultAsync(p => p.Id == vaccinationDto.PetId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            // Create the health record with RecordType="Vaccination"
            var healthRecord = new HealthRecord
            {
                PetId = vaccinationDto.PetId,
                RecordType = "Vaccination",
                Description = vaccinationDto.Description,
                RecordDate = vaccinationDto.RecordDate,
                DueDate = vaccinationDto.DueDate,
                VeterinarianId = vaccinationDto.VeterinarianId,
                Notes = vaccinationDto.Notes,
                Attachments = vaccinationDto.Attachments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.HealthRecords.Add(healthRecord);
            await db.SaveChangesAsync();

            // Reload the health record with related entities
            healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == healthRecord.Id);

            return Results.Created($"/vaccinations/{healthRecord.Id}", mapper.Map<VaccinationDto>(healthRecord));
        }).RequireAuthorization();

        // Update a vaccination
        app.MapPut("/vaccinations/{id}", async (
            int id,
            [FromBody] VaccinationUpdateDto vaccinationDto,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var vaccination = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id && hr.RecordType == "Vaccination");

            if (vaccination == null)
            {
                return Results.NotFound("Vaccination record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = vaccination.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            // Update the vaccination record
            vaccination.Description = vaccinationDto.Description;
            vaccination.RecordDate = vaccinationDto.RecordDate;
            vaccination.DueDate = vaccinationDto.DueDate;
            vaccination.VeterinarianId = vaccinationDto.VeterinarianId;
            vaccination.Notes = vaccinationDto.Notes;
            vaccination.Attachments = vaccinationDto.Attachments;
            vaccination.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Reload the vaccination with related entities
            vaccination = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == vaccination.Id);

            return Results.Ok(mapper.Map<VaccinationDto>(vaccination));
        }).RequireAuthorization();

        // Delete a vaccination
        app.MapDelete("/vaccinations/{id}", async (
            int id,
            ClaimsPrincipal user,
            PetPalDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var vaccination = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id && hr.RecordType == "Vaccination");

            if (vaccination == null)
            {
                return Results.NotFound("Vaccination record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = vaccination.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            // Delete the vaccination record
            db.HealthRecords.Remove(vaccination);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        // Get upcoming vaccination due dates
        app.MapGet("/pets/{petId}/vaccinations/upcoming", async (
            int petId,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper,
            [FromQuery] int daysAhead = 30) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            var pet = await db.Pets
                .Include(p => p.Owners)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Check if the user is an admin, vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(daysAhead);

            var upcomingVaccinations = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.PetId == petId &&
                       hr.RecordType == "Vaccination" &&
                       hr.DueDate != null &&
                       hr.DueDate >= today &&
                       hr.DueDate <= endDate)
                .OrderBy(hr => hr.DueDate)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<VaccinationDto>>(upcomingVaccinations));
        }).RequireAuthorization();

        // Get all upcoming vaccination due dates for all pets owned by the user
        app.MapGet("/vaccinations/upcoming", async (
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper,
            [FromQuery] int daysAhead = 30) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            // Get all pets owned by the user
            var userPets = await db.Pets
                .Where(p => p.Owners.Any(o => o.UserProfileId == userProfile.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (!userPets.Any() && !user.IsInRole("Admin") && !user.IsInRole("Veterinarian"))
            {
                return Results.Ok(new List<VaccinationDto>());
            }

            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(daysAhead);

            IQueryable<HealthRecord> query = db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.RecordType == "Vaccination" &&
                       hr.DueDate != null &&
                       hr.DueDate >= today &&
                       hr.DueDate <= endDate);

            // Filter by user's pets unless admin or vet
            if (!user.IsInRole("Admin") && !user.IsInRole("Veterinarian"))
            {
                query = query.Where(hr => userPets.Contains(hr.PetId));
            }

            var upcomingVaccinations = await query
                .OrderBy(hr => hr.DueDate)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<VaccinationDto>>(upcomingVaccinations));
        }).RequireAuthorization();
    }
}