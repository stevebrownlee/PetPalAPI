using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class WeightEndpoints
{
    public static void MapWeightEndpoints(this WebApplication app)
    {
        // Get all weight records for a specific pet
        app.MapGet("/pets/{petId}/weights", async (
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

            var weights = await db.Weights
                .Include(w => w.Pet)
                .Where(w => w.PetId == petId)
                .OrderByDescending(w => w.Date)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<WeightDto>>(weights));
        }).RequireAuthorization();

        // Get weight record by ID
        app.MapGet("/weights/{id}", async (
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

            var weight = await db.Weights
                .Include(w => w.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weight == null)
            {
                return Results.NotFound("Weight record not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = weight.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<WeightDto>(weight));
        }).RequireAuthorization();

        // Create a new weight record
        app.MapPost("/weights", async (
            [FromBody] WeightCreateDto weightDto,
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
                .FirstOrDefaultAsync(p => p.Id == weightDto.PetId);

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

            // Create the weight record
            var weight = mapper.Map<Weight>(weightDto);

            // Set the current date if not provided
            if (weight.Date == default)
            {
                weight.Date = DateTime.UtcNow;
            }

            db.Weights.Add(weight);
            await db.SaveChangesAsync();

            // Update the pet's current weight
            pet.Weight = weight.WeightValue;
            pet.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // Reload the weight with pet
            weight = await db.Weights
                .Include(w => w.Pet)
                .FirstOrDefaultAsync(w => w.Id == weight.Id);

            return Results.Created($"/weights/{weight.Id}", mapper.Map<WeightDto>(weight));
        }).RequireAuthorization();

        // Update a weight record
        app.MapPut("/weights/{id}", async (
            int id,
            [FromBody] WeightUpdateDto weightDto,
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

            var weight = await db.Weights
                .Include(w => w.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weight == null)
            {
                return Results.NotFound("Weight record not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = weight.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Update the weight record
            mapper.Map(weightDto, weight);
            weight.UpdatedAt = DateTime.UtcNow;

            // If this is the most recent weight record, update the pet's current weight
            var latestWeight = await db.Weights
                .Where(w => w.PetId == weight.PetId)
                .OrderByDescending(w => w.Date)
                .FirstOrDefaultAsync();

            if (latestWeight != null && latestWeight.Id == weight.Id)
            {
                var pet = await db.Pets.FindAsync(weight.PetId);
                if (pet != null)
                {
                    pet.Weight = weight.WeightValue;
                    pet.UpdatedAt = DateTime.UtcNow;
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<WeightDto>(weight));
        }).RequireAuthorization();

        // Delete a weight record
        app.MapDelete("/weights/{id}", async (
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

            var weight = await db.Weights
                .Include(w => w.Pet)
                .ThenInclude(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weight == null)
            {
                return Results.NotFound("Weight record not found.");
            }

            // Check if the user is an admin or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isPetOwner = weight.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            if (!isAdmin && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Delete the weight record
            db.Weights.Remove(weight);

            // If this was the most recent weight record, update the pet's current weight to the next most recent
            var petId = weight.PetId;
            await db.SaveChangesAsync();

            var latestWeight = await db.Weights
                .Where(w => w.PetId == petId)
                .OrderByDescending(w => w.Date)
                .FirstOrDefaultAsync();

            if (latestWeight != null)
            {
                var pet = await db.Pets.FindAsync(petId);
                if (pet != null)
                {
                    pet.Weight = latestWeight.WeightValue;
                    pet.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            return Results.NoContent();
        }).RequireAuthorization();

        // Get weight history for a pet in a format suitable for graphing
        app.MapGet("/pets/{petId}/weight-history", async (
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

            var weightHistory = await db.Weights
                .Where(w => w.PetId == petId)
                .OrderBy(w => w.Date)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<WeightHistoryDto>>(weightHistory));
        }).RequireAuthorization();
    }
}