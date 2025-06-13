using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
    {
        // Get notification settings for current user
        app.MapGet("/settings/notifications", async (
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

            var notificationSettings = await db.NotificationSettings
                .FirstOrDefaultAsync(ns => ns.UserId == userProfile.Id);

            if (notificationSettings == null)
            {
                // Create default notification settings if none exist
                notificationSettings = new NotificationSettings
                {
                    UserId = userProfile.Id,
                    UserProfile = userProfile
                };

                db.NotificationSettings.Add(notificationSettings);
                await db.SaveChangesAsync();
            }

            return Results.Ok(mapper.Map<NotificationSettingsDto>(notificationSettings));
        }).RequireAuthorization();

        // Update notification settings for current user
        app.MapPut("/settings/notifications", async (
            [FromBody] NotificationSettingsUpdateDto settingsDto,
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

            var notificationSettings = await db.NotificationSettings
                .FirstOrDefaultAsync(ns => ns.UserId == userProfile.Id);

            if (notificationSettings == null)
            {
                // Create new notification settings if none exist
                notificationSettings = new NotificationSettings
                {
                    UserId = userProfile.Id,
                    UserProfile = userProfile
                };

                db.NotificationSettings.Add(notificationSettings);
            }

            // Update notification settings
            mapper.Map(settingsDto, notificationSettings);
            notificationSettings.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<NotificationSettingsDto>(notificationSettings));
        }).RequireAuthorization();

        // Get theme preference for current user
        app.MapGet("/settings/theme", async (
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

            return Results.Ok(mapper.Map<ThemePreferenceDto>(userProfile));
        }).RequireAuthorization();

        // Update theme preference for current user
        app.MapPut("/settings/theme", async (
            [FromBody] ThemePreferenceUpdateDto themeDto,
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

            // Update theme preference
            mapper.Map(themeDto, userProfile);
            userProfile.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(mapper.Map<ThemePreferenceDto>(userProfile));
        }).RequireAuthorization();
    }
}