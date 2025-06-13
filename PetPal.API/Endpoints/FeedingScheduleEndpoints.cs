using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class FeedingScheduleEndpoints
{
    public static void MapFeedingScheduleEndpoints(this WebApplication app)
    {
        // Get all feeding schedules for a specific pet
        app.MapGet("/pets/{petId}/feeding-schedules", async (
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

            var feedingSchedules = await db.FeedingSchedules
                .Include(fs => fs.Pet)
                .Where(fs => fs.PetId == petId)
                .OrderBy(fs => fs.FeedingTime)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<FeedingScheduleDto>>(feedingSchedules));
        }).RequireAuthorization();

        // Get feeding schedule by ID
        app.MapGet("/feeding-schedules/{id}", async (
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

            var feedingSchedule = await db.FeedingSchedules
                .Include(fs => fs.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (feedingSchedule == null)
            {
                return Results.NotFound("Feeding schedule not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = feedingSchedule.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<FeedingScheduleDto>(feedingSchedule));
        }).RequireAuthorization();

        // Create a new feeding schedule
        app.MapPost("/feeding-schedules", async (
            [FromBody] FeedingScheduleCreateDto feedingScheduleDto,
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
                .FirstOrDefaultAsync(p => p.Id == feedingScheduleDto.PetId);

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

            // Create the feeding schedule
            var feedingSchedule = mapper.Map<FeedingSchedule>(feedingScheduleDto);

            db.FeedingSchedules.Add(feedingSchedule);
            await db.SaveChangesAsync();

            // Reload the feeding schedule with pet
            feedingSchedule = await db.FeedingSchedules
                .Include(fs => fs.Pet)
                .FirstOrDefaultAsync(fs => fs.Id == feedingSchedule.Id);

            return Results.Created($"/feeding-schedules/{feedingSchedule.Id}", mapper.Map<FeedingScheduleDto>(feedingSchedule));
        }).RequireAuthorization();

        // Update a feeding schedule
        app.MapPut("/feeding-schedules/{id}", async (
            int id,
            [FromBody] FeedingScheduleUpdateDto feedingScheduleDto,
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

            var feedingSchedule = await db.FeedingSchedules
                .Include(fs => fs.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (feedingSchedule == null)
            {
                return Results.NotFound("Feeding schedule not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = feedingSchedule.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Update the feeding schedule
            mapper.Map(feedingScheduleDto, feedingSchedule);
            feedingSchedule.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<FeedingScheduleDto>(feedingSchedule));
        }).RequireAuthorization();

        // Delete a feeding schedule
        app.MapDelete("/feeding-schedules/{id}", async (
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

            var feedingSchedule = await db.FeedingSchedules
                .Include(fs => fs.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (feedingSchedule == null)
            {
                return Results.NotFound("Feeding schedule not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = feedingSchedule.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Delete the feeding schedule
            db.FeedingSchedules.Remove(feedingSchedule);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();
    }
}