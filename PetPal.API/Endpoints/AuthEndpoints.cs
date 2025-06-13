using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace PetPal.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Registration endpoint
        app.MapPost("/auth/register", async (
            [FromBody] RegistrationDto registration,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            PetPalDbContext db,
            IMapper mapper,
            SignInManager<IdentityUser> signInManager) =>
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(registration.Email);
            if (existingUser != null)
            {
                return Results.Conflict("A user with this email already exists.");
            }

            // Create the Identity user
            var identityUser = new IdentityUser
            {
                UserName = registration.Email,
                Email = registration.Email,
                EmailConfirmed = true // For simplicity, we're auto-confirming emails
            };

            var result = await userManager.CreateAsync(identityUser, registration.Password);

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            // Ensure the "User" role exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Assign the "User" role to the new user
            await userManager.AddToRoleAsync(identityUser, "User");

            // Create the UserProfile
            var userProfile = mapper.Map<UserProfile>(registration);
            userProfile.IdentityUserId = identityUser.Id;

            db.UserProfiles.Add(userProfile);
            await db.SaveChangesAsync();

            // Sign in the user
            await signInManager.SignInAsync(identityUser, isPersistent: false);

            // Return the user profile
            var userProfileDto = mapper.Map<UserProfileDto>(userProfile);
            userProfileDto.Roles = new List<string> { "User" };

            return Results.Created($"/api/users/{userProfile.Id}", userProfileDto);
        });

        // Login endpoint
        app.MapPost("/auth/login", async (
            [FromBody] LoginDto login,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var identityUser = await userManager.FindByEmailAsync(login.Email);
            if (identityUser == null)
            {
                return Results.Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(identityUser, login.Password, false);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            // Get the user profile
            var userProfile = await db.UserProfiles.FirstOrDefaultAsync(up => up.IdentityUserId == identityUser.Id);

            // If user profile doesn't exist but user is authenticated, create one
            if (userProfile == null)
            {
                // Check if user has Admin role
                var userRoles = await userManager.GetRolesAsync(identityUser);
                if (userRoles.Contains("Admin"))
                {
                    // Create a profile for the admin user
                    userProfile = new Models.UserProfile
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = identityUser.Email,
                        Address = "Admin Address",
                        Phone = "Admin Phone",
                        IdentityUserId = identityUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    db.UserProfiles.Add(userProfile);
                    await db.SaveChangesAsync();
                }
                else
                {
                    return Results.Unauthorized();
                }
            }

            // Get the user's roles
            var roles = await userManager.GetRolesAsync(identityUser);

            // Sign in the user
            await signInManager.SignInAsync(identityUser, isPersistent: true);

            // Return the user profile
            var userProfileDto = mapper.Map<UserProfileDto>(userProfile);
            userProfileDto.Roles = roles.ToList();

            return Results.Ok(userProfileDto);
        });

        // Logout endpoint
        app.MapPost("/auth/logout", async (SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.NoContent();
        });

        // Get current user info
        app.MapGet("/auth/me", async (
            ClaimsPrincipal user,
            UserManager<IdentityUser> userManager,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var identityUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
            {
                return Results.Unauthorized();
            }

            var identityUser = await userManager.FindByIdAsync(identityUserId);
            if (identityUser == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles.FirstOrDefaultAsync(up => up.IdentityUserId == identityUserId);

            // If user profile doesn't exist but user is authenticated, create one
            if (userProfile == null)
            {
                // Check if user has Admin role
                var userRoles = await userManager.GetRolesAsync(identityUser);
                if (userRoles.Contains("Admin"))
                {
                    // Create a profile for the admin user
                    userProfile = new Models.UserProfile
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = identityUser.Email,
                        Address = "Admin Address",
                        Phone = "Admin Phone",
                        IdentityUserId = identityUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    db.UserProfiles.Add(userProfile);
                    await db.SaveChangesAsync();
                }
                else
                {
                    return Results.NotFound("User profile not found.");
                }
            }

            // Get the user's roles
            var roles = await userManager.GetRolesAsync(identityUser);

            // Return the user profile
            var userProfileDto = mapper.Map<UserProfileDto>(userProfile);
            userProfileDto.Roles = roles.ToList();

            return Results.Ok(userProfileDto);
        }).RequireAuthorization();

        // Update user profile endpoint
        app.MapPut("/auth/profile", async (
            [FromBody] UpdateUserProfileDto updateProfileDto,
            ClaimsPrincipal user,
            UserManager<IdentityUser> userManager,
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var identityUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
            {
                return Results.Unauthorized();
            }

            var userProfile = await db.UserProfiles.FirstOrDefaultAsync(up => up.IdentityUserId == identityUserId);
            if (userProfile == null)
            {
                return Results.NotFound("User profile not found.");
            }

            // Update the user profile
            userProfile.FirstName = updateProfileDto.FirstName;
            userProfile.LastName = updateProfileDto.LastName;
            userProfile.Address = updateProfileDto.Address;
            userProfile.Phone = updateProfileDto.Phone;
            userProfile.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Return the updated user profile
            var userProfileDto = mapper.Map<UserProfileDto>(userProfile);
            var identityUser = await userManager.FindByIdAsync(identityUserId);
            var roles = await userManager.GetRolesAsync(identityUser);
            userProfileDto.Roles = roles.ToList();

            return Results.Ok(userProfileDto);
        }).RequireAuthorization();

        // Forgot password endpoint
        app.MapPost("/auth/forgot-password", async (
            [FromBody] ForgotPasswordDto forgotPasswordDto,
            UserManager<IdentityUser> userManager,
            ILogger<Program> logger) =>
        {
            var user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Results.Ok(new { message = "If your email is registered, you will receive a password reset link." });
            }

            // Generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token for URL safety
            var encodedToken = HttpUtility.UrlEncode(token);

            // In a real application, you would send an email with a link to reset the password
            // For development, we'll log the token and simulate the email sending

            // Create a reset link (this would be included in the email)
            var resetLink = $"https://petpal.example.com/reset-password?email={HttpUtility.UrlEncode(forgotPasswordDto.Email)}&token={encodedToken}";

            // Log the reset link (for development purposes only)
            logger.LogInformation($"Password reset link for {forgotPasswordDto.Email}: {resetLink}");

            // In a real application, you would send an email here
            // For development, we'll just simulate it
            logger.LogInformation($"Simulating sending password reset email to {forgotPasswordDto.Email}");

            return Results.Ok(new { message = "If your email is registered, you will receive a password reset link." });
        });

        // Reset password endpoint
        app.MapPost("/auth/reset-password", async (
            [FromBody] ResetPasswordDto resetPasswordDto,
            UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Results.BadRequest(new { message = "Invalid token or email." });
            }

            // Reset the password
            var result = await userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { message = "Failed to reset password.", errors = result.Errors });
            }

            return Results.Ok(new { message = "Password has been reset successfully." });
        });
    }
}