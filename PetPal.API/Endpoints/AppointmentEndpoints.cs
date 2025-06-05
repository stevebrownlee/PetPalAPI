using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this WebApplication app)
    {
        // Get all appointments (admin only)
        app.MapGet("/appointments", async (
            PetPalDbContext db,
            IMapper mapper) =>
        {
            var appointments = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<AppointmentDto>>(appointments));
        }).RequireAuthorization("AdminOnly");

        // Get appointments for current user
        app.MapGet("/user/appointments", async (
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

            // If user is a veterinarian, get all appointments (in a real app, you'd filter by the vet's ID)
            if (user.IsInRole("Veterinarian"))
            {
                var vetAppointments = await db.Appointments
                    .Include(a => a.Pet)
                    .Include(a => a.Veterinarian)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToListAsync();

                return Results.Ok(mapper.Map<List<AppointmentDto>>(vetAppointments));
            }

            // For pet owners, get appointments for their pets
            var petOwners = await db.PetOwners
                .Where(po => po.UserProfileId == userProfile.Id)
                .Select(po => po.PetId)
                .ToListAsync();

            var appointments = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Where(a => petOwners.Contains(a.PetId))
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<AppointmentDto>>(appointments));
        }).RequireAuthorization();

        // Get appointments for a specific pet
        app.MapGet("/pets/{petId}/appointments", async (
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

            var appointments = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<AppointmentDto>>(appointments));
        }).RequireAuthorization();

        // Get a specific appointment
        app.MapGet("/appointments/{id}", async (
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

            var appointment = await db.Appointments
                .Include(a => a.Pet)
                .ThenInclude(p => p.Owners)
                .Include(a => a.Veterinarian)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return Results.NotFound("Appointment not found.");
            }

            // Check if the user is an admin, a vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = appointment.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            // For now, any vet can view any appointment
            // In a real application, you might want to check if this specific vet is assigned to this appointment

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            return Results.Ok(mapper.Map<AppointmentDto>(appointment));
        }).RequireAuthorization();

        // Create a new appointment
        app.MapPost("/appointments", async (
            [FromBody] AppointmentCreateDto appointmentDto,
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

            // Verify the pet exists
            var pet = await db.Pets
                .Include(p => p.Owners)
                .FirstOrDefaultAsync(p => p.Id == appointmentDto.PetId);

            if (pet == null)
            {
                return Results.NotFound("Pet not found.");
            }

            // Verify the veterinarian exists
            var veterinarian = await db.Veterinarians
                .FirstOrDefaultAsync(v => v.Id == appointmentDto.VeterinarianId);

            if (veterinarian == null)
            {
                return Results.NotFound("Veterinarian not found.");
            }

            // Check if the user is an admin, a vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            // For now, any vet can create an appointment
            // In a real application, you might want to check if this specific vet is assigned to this appointment

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Create the appointment
            var appointment = mapper.Map<Appointment>(appointmentDto);
            appointment.Status = "Scheduled"; // Default status for new appointments

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            // Reload the appointment with related entities
            appointment = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            return Results.Created($"/appointments/{appointment.Id}", mapper.Map<AppointmentDto>(appointment));
        }).RequireAuthorization();

        // Update an appointment
        app.MapPut("/appointments/{id}", async (
            int id,
            [FromBody] AppointmentUpdateDto appointmentDto,
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

            var appointment = await db.Appointments
                .Include(a => a.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return Results.NotFound("Appointment not found.");
            }

            // Verify the veterinarian exists
            var veterinarian = await db.Veterinarians
                .FirstOrDefaultAsync(v => v.Id == appointmentDto.VeterinarianId);

            if (veterinarian == null)
            {
                return Results.NotFound("Veterinarian not found.");
            }

            // Check if the user is an admin, a vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = appointment.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            // For now, any vet can update any appointment
            // In a real application, you might want to check if this specific vet is assigned to this appointment

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Update the appointment
            mapper.Map(appointmentDto, appointment);
            appointment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Reload the appointment with related entities
            appointment = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            return Results.Ok(mapper.Map<AppointmentDto>(appointment));
        }).RequireAuthorization();

        // Delete an appointment
        app.MapDelete("/appointments/{id}", async (
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

            var appointment = await db.Appointments
                .Include(a => a.Pet)
                .ThenInclude(p => p.Owners)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return Results.NotFound("Appointment not found.");
            }

            // Check if the user is an admin, a vet, or owns the pet
            var isAdmin = user.IsInRole("Admin");
            var isVet = user.IsInRole("Veterinarian");
            var isPetOwner = appointment.Pet.Owners.Any(po => po.UserProfileId == userProfile.Id);

            // For now, any vet can delete any appointment
            // In a real application, you might want to check if this specific vet is assigned to this appointment

            if (!isAdmin && !isVet && !isPetOwner)
            {
                return Results.Forbid();
            }

            // Delete the appointment
            db.Appointments.Remove(appointment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();
    }
}