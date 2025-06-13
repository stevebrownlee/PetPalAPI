using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class MedicationEndpoints
{
    public static void MapMedicationEndpoints(this WebApplication app)
    {
        // Get all medications for a specific pet
        app.MapGet("/pets/{petId}/medications", async (
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
                .Include(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            var medications = await db.Medications
                .Include(m => m.Pet)
                .Where(m => m.PetId == petId)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<MedicationDto>>(medications));
        }).RequireAuthorization();

        // Get medication by ID
        app.MapGet("/medications/{id}", async (
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

            var medication = await db.Medications
                .Include(m => m.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return Results.NotFound("Medication not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = medication.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<MedicationDto>(medication));
        }).RequireAuthorization();

        // Create a new medication
        app.MapPost("/medications", async (
            [FromBody] MedicationCreateDto medicationDto,
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
                .Include(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(p => p.Id == medicationDto.PetId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Create the medication
            var medication = mapper.Map<Medication>(medicationDto);

            // Calculate next reminder due date if reminders are enabled
            if (medication.ReminderEnabled && medication.ReminderTime.HasValue)
            {
                // Set initial next reminder due date
                var now = DateTime.UtcNow;
                var today = now.Date;
                var reminderTimeToday = today.Add(medication.ReminderTime.Value);

                medication.NextReminderDue = reminderTimeToday > now
                    ? reminderTimeToday
                    : reminderTimeToday.AddDays(1); // If today's time has passed, set for tomorrow
            }

            db.Medications.Add(medication);
            await db.SaveChangesAsync();

            // Reload the medication with pet
            medication = await db.Medications
                .Include(m => m.Pet)
                .FirstOrDefaultAsync(m => m.Id == medication.Id);

            return Results.Created($"/medications/{medication.Id}", mapper.Map<MedicationDto>(medication));
        }).RequireAuthorization();

        // Update a medication
        app.MapPut("/medications/{id}", async (
            int id,
            [FromBody] MedicationUpdateDto medicationDto,
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

            var medication = await db.Medications
                .Include(m => m.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return Results.NotFound("Medication not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = medication.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Update the medication
            mapper.Map(medicationDto, medication);

            // Update reminder settings if needed
            if (medication.ReminderEnabled && medication.ReminderTime.HasValue)
            {
                // Recalculate next reminder due date if reminder settings changed
                var now = DateTime.UtcNow;
                var today = now.Date;
                var reminderTimeToday = today.Add(medication.ReminderTime.Value);

                medication.NextReminderDue = reminderTimeToday > now
                    ? reminderTimeToday
                    : reminderTimeToday.AddDays(1); // If today's time has passed, set for tomorrow
            }
            else if (!medication.ReminderEnabled)
            {
                medication.NextReminderDue = null;
                medication.LastReminderSent = null;
            }

            medication.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<MedicationDto>(medication));
        }).RequireAuthorization();

        // Delete a medication
        app.MapDelete("/medications/{id}", async (
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

            var medication = await db.Medications
                .Include(m => m.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return Results.NotFound("Medication not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = medication.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Delete the medication
            db.Medications.Remove(medication);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        // Update medication reminder settings
        app.MapPut("/medications/{id}/reminder", async (
            int id,
            [FromBody] MedicationReminderUpdateDto reminderDto,
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

            var medication = await db.Medications
                .Include(m => m.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return Results.NotFound("Medication not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = medication.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Update reminder settings
            medication.ReminderEnabled = reminderDto.ReminderEnabled;
            medication.ReminderFrequency = reminderDto.ReminderFrequency;
            medication.ReminderTime = reminderDto.ReminderTime;

            // Update next reminder due date if reminders are enabled
            if (medication.ReminderEnabled && medication.ReminderTime.HasValue)
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var reminderTimeToday = today.Add(medication.ReminderTime.Value);

                medication.NextReminderDue = reminderTimeToday > now
                    ? reminderTimeToday
                    : reminderTimeToday.AddDays(1); // If today's time has passed, set for tomorrow
            }
            else
            {
                medication.NextReminderDue = null;
                medication.LastReminderSent = null;
            }

            medication.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<MedicationDto>(medication));
        }).RequireAuthorization();

        // Get upcoming medication reminders for a user's pets
        app.MapGet("/user/medication-reminders", async (
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

            // Get all pets owned by the user
            var petIds = await db.PetOwners
                .Where(po => po.UserProfileId == userProfile.Id)
                .Select(po => po.PetId)
                .ToListAsync();

            // Get all active medications with reminders enabled for those pets
            var medications = await db.Medications
                .Include(m => m.Pet)
                .Where(m => petIds.Contains(m.PetId) && m.IsActive && m.ReminderEnabled)
                .OrderBy(m => m.NextReminderDue)
                .ToListAsync();

            var reminderDtos = medications.Select(m => new MedicationReminderDto
            {
                MedicationId = m.Id,
                MedicationName = m.Name,
                PetId = m.PetId,
                PetName = m.Pet.Name,
                ReminderEnabled = m.ReminderEnabled,
                ReminderFrequency = m.ReminderFrequency,
                ReminderTime = m.ReminderTime,
                NextReminderDue = m.NextReminderDue
            }).ToList();

            return Results.Ok(reminderDtos);
        }).RequireAuthorization();

        // Mark a reminder as sent (for notification system)
        app.MapPost("/medications/{id}/reminder-sent", async (
            int id,
            ClaimsPrincipal user,
            PetPalDbContext db) =>
        {
            // This endpoint would typically be called by a background service
            // But we're adding it here for completeness

            var isAdmin = user.IsInRole("Admin");
            if (!isAdmin)
            {
                return Results.Forbid(); // Only allow admin or system access
            }

            var medication = await db.Medications
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return Results.NotFound("Medication not found.");
            }

            if (!medication.ReminderEnabled)
            {
                return Results.BadRequest("Reminders are not enabled for this medication.");
            }

            // Update the last reminder sent time
            medication.LastReminderSent = DateTime.UtcNow;

            // Calculate the next reminder due date based on frequency
            if (medication.ReminderTime.HasValue)
            {
                // For simplicity, we'll just set it to the next day at the same time
                // In a real implementation, this would be more sophisticated based on ReminderFrequency
                var nextDay = DateTime.UtcNow.Date.AddDays(1);
                medication.NextReminderDue = nextDay.Add(medication.ReminderTime.Value);
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization("AdminOnly");
    }
}