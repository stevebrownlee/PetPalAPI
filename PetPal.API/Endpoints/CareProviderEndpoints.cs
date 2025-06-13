using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class CareProviderEndpoints
{
    public static void MapCareProviderEndpoints(this WebApplication app)
    {
        // Get all care providers for the current user
        app.MapGet("/care-providers", async (
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var careProviders = await db.CareProviders
                .Where(cp => cp.UserId == userId)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<CareProviderDto>>(careProviders));
        }).RequireAuthorization();

        // Get care provider by ID
        app.MapGet("/care-providers/{id}", async (
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

            var careProvider = await db.CareProviders
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (careProvider == null)
            {
                return Results.NotFound("Care provider not found.");
            }

            // Check if the user is an admin or the owner of the care provider
            var isAdmin = user.IsInRole("Admin");
            var isOwner = careProvider.UserId == userId;

            if (!isAdmin && !isOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<CareProviderDto>(careProvider));
        }).RequireAuthorization();

        // Create a new care provider
        app.MapPost("/care-providers", async (
            [FromBody] CareProviderCreateDto careProviderDto,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var careProvider = mapper.Map<CareProvider>(careProviderDto);
            careProvider.UserId = userId;
            careProvider.CreatedAt = DateTime.UtcNow;
            careProvider.UpdatedAt = DateTime.UtcNow;

            db.CareProviders.Add(careProvider);
            await db.SaveChangesAsync();

            return Results.Created($"/care-providers/{careProvider.Id}", mapper.Map<CareProviderDto>(careProvider));
        }).RequireAuthorization();

        // Update a care provider
        app.MapPut("/care-providers/{id}", async (
            int id,
            [FromBody] CareProviderUpdateDto careProviderDto,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var careProvider = await db.CareProviders
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (careProvider == null)
            {
                return Results.NotFound("Care provider not found.");
            }

            // Check if the user is an admin or the owner of the care provider
            var isAdmin = user.IsInRole("Admin");
            var isOwner = careProvider.UserId == userId;

            if (!isAdmin && !isOwner)
            {
                return Results.Forbid();
            }

            // Update the care provider
            mapper.Map(careProviderDto, careProvider);
            careProvider.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<CareProviderDto>(careProvider));
        }).RequireAuthorization();

        // Delete a care provider
        app.MapDelete("/care-providers/{id}", async (
            int id,
            ClaimsPrincipal user,
            PetPalDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var careProvider = await db.CareProviders
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (careProvider == null)
            {
                return Results.NotFound("Care provider not found.");
            }

            // Check if the user is an admin or the owner of the care provider
            var isAdmin = user.IsInRole("Admin");
            var isOwner = careProvider.UserId == userId;

            if (!isAdmin && !isOwner)
            {
                return Results.Forbid();
            }

            // Delete the care provider
            db.CareProviders.Remove(careProvider);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();
    }
}