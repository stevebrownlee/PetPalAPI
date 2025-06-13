using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using PetPal.API.Services;
using System.Security.Claims;
using System.Text.Json;

namespace PetPal.API.Endpoints;

public static class HealthRecordEndpoints
{
    public static void MapHealthRecordEndpoints(this WebApplication app)
    {
        // Get all health records for a pet
        app.MapGet("/pets/{petId}/healthrecords", async (
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

            var healthRecords = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.PetId == petId)
                .OrderByDescending(hr => hr.RecordDate)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<HealthRecordDto>>(healthRecords));
        }).RequireAuthorization();

        // Get a specific health record
        app.MapGet("/healthrecords/{id}", async (
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

            var healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == id);

            if (healthRecord == null)
            {
                return Results.NotFound("Health record not found.");
            }

            // Check if the user is an admin, vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = healthRecord.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<HealthRecordDto>(healthRecord));
        }).RequireAuthorization();

        // Create a new health record
        app.MapPost("/healthrecords", async (
            [FromBody] HealthRecordCreateDto healthRecordDto,
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
                .FirstOrDefaultAsync(p => p.Id == healthRecordDto.PetId);

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

            // Create the health record
            var healthRecord = mapper.Map<HealthRecord>(healthRecordDto);
            db.HealthRecords.Add(healthRecord);
            await db.SaveChangesAsync();

            // Reload the health record with related entities
            healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == healthRecord.Id);

            return Results.Created($"/healthrecords/{healthRecord.Id}", mapper.Map<HealthRecordDto>(healthRecord));
        }).RequireAuthorization();

        // Update a health record
        app.MapPut("/healthrecords/{id}", async (
            int id,
            [FromBody] HealthRecordUpdateDto healthRecordDto,
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

            var healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id);

            if (healthRecord == null)
            {
                return Results.NotFound("Health record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = healthRecord.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            // Update the health record
            mapper.Map(healthRecordDto, healthRecord);
            healthRecord.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Reload the health record with related entities
            healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .FirstOrDefaultAsync(hr => hr.Id == healthRecord.Id);

            return Results.Ok(mapper.Map<HealthRecordDto>(healthRecord));
        }).RequireAuthorization();

        // Delete a health record
        app.MapDelete("/healthrecords/{id}", async (
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

            var healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id);

            if (healthRecord == null)
            {
                return Results.NotFound("Health record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = healthRecord.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            // Delete the health record
            db.HealthRecords.Remove(healthRecord);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        // Upload document to a health record
        app.MapPost("/healthrecords/{id}/documents", async (
            int id,
            IFormFile document,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IFileStorageService fileStorage,
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

            var healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id);

            if (healthRecord == null)
            {
                return Results.NotFound("Health record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = healthRecord.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            try
            {
                // Save document
                var fileName = await fileStorage.SaveFileAsync(document, "uploads/documents");
                var fileUrl = fileStorage.GetFileUrl(fileName, "uploads/documents");

                // Update health record attachments
                // If attachments already exist, add the new one to the list
                List<string> attachments = new List<string>();

                if (!string.IsNullOrEmpty(healthRecord.Attachments))
                {
                    try
                    {
                        attachments = JsonSerializer.Deserialize<List<string>>(healthRecord.Attachments);
                    }
                    catch
                    {
                        // If not a valid JSON array, treat as a single URL
                        if (!string.IsNullOrEmpty(healthRecord.Attachments))
                        {
                            attachments.Add(healthRecord.Attachments);
                        }
                    }
                }

                attachments.Add(fileUrl);
                healthRecord.Attachments = JsonSerializer.Serialize(attachments);
                healthRecord.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.Ok(mapper.Map<HealthRecordDto>(healthRecord));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error uploading document: {ex.Message}");
            }
        }).RequireAuthorization()
        .DisableAntiforgery(); // Disable antiforgery for file uploads

        // Upload multiple documents to a health record
        app.MapPost("/healthrecords/{id}/documents/multiple", async (
            int id,
            [FromForm] IFormFileCollection documents,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IFileStorageService fileStorage,
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

            var healthRecord = await db.HealthRecords
                .Include(hr => hr.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(hr => hr.Id == id);

            if (healthRecord == null)
            {
                return Results.NotFound("Health record not found.");
            }

            // Check if the user is an admin, vet, or the primary owner of the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPrimaryOwner = healthRecord.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id && po.IsPrimaryOwner);

            if (!isAdmin && !isVet && !isPrimaryOwner)
            {
                return Results.Forbid();
            }

            try
            {
                // Parse existing attachments
                List<string> attachments = new List<string>();

                if (!string.IsNullOrEmpty(healthRecord.Attachments))
                {
                    try
                    {
                        attachments = JsonSerializer.Deserialize<List<string>>(healthRecord.Attachments);
                    }
                    catch
                    {
                        // If not a valid JSON array, treat as a single URL
                        if (!string.IsNullOrEmpty(healthRecord.Attachments))
                        {
                            attachments.Add(healthRecord.Attachments);
                        }
                    }
                }

                // Save each document
                foreach (var document in documents)
                {
                    var fileName = await fileStorage.SaveFileAsync(document, "uploads/documents");
                    var fileUrl = fileStorage.GetFileUrl(fileName, "uploads/documents");
                    attachments.Add(fileUrl);
                }

                // Update health record
                healthRecord.Attachments = JsonSerializer.Serialize(attachments);
                healthRecord.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.Ok(mapper.Map<HealthRecordDto>(healthRecord));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error uploading documents: {ex.Message}");
            }
        }).RequireAuthorization()
        .DisableAntiforgery(); // Disable antiforgery for file uploads
    }
}