using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;

namespace PetPal.API.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        // Get user dashboard data
        app.MapGet("/dashboard", async (
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

            // Get basic pet information
            var pets = await db.Pets
                .Where(p => petIds.Contains(p.Id))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var oneMonthFromNow = now.AddMonths(1);

            // Get upcoming appointments
            var upcomingAppointments = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Where(a => petIds.Contains(a.PetId) &&
                       a.AppointmentDate >= now &&
                       a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .Take(10)
                .ToListAsync();

            // Get active medications
            var activeMedications = await db.Medications
                .Include(m => m.Pet)
                .Where(m => petIds.Contains(m.PetId) &&
                       m.IsActive &&
                       (m.EndDate == null || m.EndDate >= now))
                .OrderBy(m => m.NextReminderDue)
                .Take(10)
                .ToListAsync();

            // Get upcoming vaccinations
            var upcomingVaccinations = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => petIds.Contains(hr.PetId) &&
                       hr.RecordType == "Vaccination" &&
                       hr.DueDate.HasValue &&
                       hr.DueDate >= now &&
                       hr.DueDate <= oneMonthFromNow)
                .OrderBy(hr => hr.DueDate)
                .Take(10)
                .ToListAsync();

            // Create calendar events from appointments, medications, and vaccinations
            var calendarEvents = new List<CalendarEventDto>();

            // Add appointments to calendar events
            foreach (var appointment in upcomingAppointments)
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = appointment.Id,
                    EventType = "Appointment",
                    Title = $"{appointment.AppointmentType} - {appointment.Pet.Name}",
                    Description = appointment.Notes ?? string.Empty,
                    EventDate = appointment.AppointmentDate,
                    EventTime = appointment.AppointmentTime,
                    PetId = appointment.PetId,
                    PetName = appointment.Pet.Name,
                    Color = "#4285F4" // Blue
                });
            }

            // Add medication reminders to calendar events
            foreach (var medication in activeMedications.Where(m => m.ReminderEnabled && m.NextReminderDue.HasValue))
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = medication.Id,
                    EventType = "Medication",
                    Title = $"Medication: {medication.Name} - {medication.Pet.Name}",
                    Description = $"{medication.Dosage}, {medication.Instructions}",
                    EventDate = medication.NextReminderDue.Value.Date,
                    EventTime = medication.ReminderTime,
                    PetId = medication.PetId,
                    PetName = medication.Pet.Name,
                    Color = "#EA4335" // Red
                });
            }

            // Add vaccination due dates to calendar events
            foreach (var vaccination in upcomingVaccinations)
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = vaccination.Id,
                    EventType = "Vaccination",
                    Title = $"Vaccination Due: {vaccination.Description} - {vaccination.Pet.Name}",
                    Description = vaccination.Notes ?? string.Empty,
                    EventDate = vaccination.DueDate.Value,
                    EventTime = null,
                    PetId = vaccination.PetId,
                    PetName = vaccination.Pet.Name,
                    Color = "#FBBC05" // Yellow
                });
            }

            // Create pet dashboard summaries
            var petSummaries = new List<PetDashboardSummaryDto>();
            foreach (var pet in pets)
            {
                petSummaries.Add(new PetDashboardSummaryDto
                {
                    PetId = pet.Id,
                    PetName = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    ImageUrl = pet.ImageUrl,
                    UpcomingAppointmentsCount = upcomingAppointments.Count(a => a.PetId == pet.Id),
                    ActiveMedicationsCount = activeMedications.Count(m => m.PetId == pet.Id),
                    UpcomingVaccinationsCount = upcomingVaccinations.Count(v => v.PetId == pet.Id)
                });
            }

            // Create the user dashboard DTO
            var dashboard = new UserDashboardDto
            {
                UserProfileId = userProfile.Id,
                UserName = userProfile.FirstName + " " + userProfile.LastName,
                Pets = petSummaries,
                UpcomingEvents = calendarEvents.OrderBy(e => e.EventDate).ThenBy(e => e.EventTime).ToList(),
                TotalPets = pets.Count,
                TotalUpcomingAppointments = upcomingAppointments.Count,
                TotalActiveMedications = activeMedications.Count,
                TotalUpcomingVaccinations = upcomingVaccinations.Count
            };

            return Results.Ok(dashboard);
        }).RequireAuthorization();

        // Get pet dashboard data
        app.MapGet("/pets/{petId}/dashboard", async (
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

            var now = DateTime.UtcNow;
            var oneMonthFromNow = now.AddMonths(1);

            // Get recent health records
            var recentHealthRecords = await db.HealthRecords
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.PetId == petId)
                .OrderByDescending(hr => hr.RecordDate)
                .Take(5)
                .ToListAsync();

            // Get active medications
            var activeMedications = await db.Medications
                .Where(m => m.PetId == petId &&
                       m.IsActive &&
                       (m.EndDate == null || m.EndDate >= now))
                .OrderBy(m => m.NextReminderDue)
                .Take(5)
                .ToListAsync();

            // Get upcoming appointments
            var upcomingAppointments = await db.Appointments
                .Include(a => a.Veterinarian)
                .Where(a => a.PetId == petId &&
                       a.AppointmentDate >= now &&
                       a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .Take(5)
                .ToListAsync();

            // Get recent weight records
            var recentWeightRecords = await db.Weights
                .Where(w => w.PetId == petId)
                .OrderByDescending(w => w.Date)
                .Take(5)
                .ToListAsync();

            // Get upcoming vaccinations
            var upcomingVaccinations = await db.HealthRecords
                .Include(hr => hr.Veterinarian)
                .Where(hr => hr.PetId == petId &&
                       hr.RecordType == "Vaccination" &&
                       hr.DueDate.HasValue &&
                       hr.DueDate >= now &&
                       hr.DueDate <= oneMonthFromNow)
                .OrderBy(hr => hr.DueDate)
                .Take(5)
                .ToListAsync();

            // Create the pet dashboard DTO
            var dashboard = new PetDashboardDto
            {
                PetId = pet.Id,
                PetName = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                DateOfBirth = pet.DateOfBirth,
                CurrentWeight = pet.Weight,
                ImageUrl = pet.ImageUrl,
                UpcomingAppointmentsCount = upcomingAppointments.Count,
                ActiveMedicationsCount = activeMedications.Count,
                UpcomingVaccinationsCount = upcomingVaccinations.Count,
                RecentHealthRecords = recentHealthRecords.Select(hr => new HealthRecordSummaryDto
                {
                    Id = hr.Id,
                    RecordType = hr.RecordType,
                    Description = hr.Description,
                    RecordDate = hr.RecordDate,
                    DueDate = hr.DueDate,
                    VeterinarianName = hr.Veterinarian != null ? $"{hr.Veterinarian.FirstName} {hr.Veterinarian.LastName}" : null
                }).ToList(),
                ActiveMedications = activeMedications.Select(m => new MedicationSummaryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    ReminderEnabled = m.ReminderEnabled,
                    NextReminderDue = m.NextReminderDue
                }).ToList(),
                UpcomingAppointments = upcomingAppointments.Select(a => new AppointmentSummaryDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    AppointmentType = a.AppointmentType,
                    VeterinarianName = a.Veterinarian != null ? $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}" : null,
                    Status = a.Status
                }).ToList(),
                RecentWeightRecords = recentWeightRecords.Select(w => new WeightSummaryDto
                {
                    Id = w.Id,
                    WeightValue = w.WeightValue,
                    RecordDate = w.Date
                }).ToList()
            };

            return Results.Ok(dashboard);
        }).RequireAuthorization();

        // Get calendar events
        app.MapGet("/calendar", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
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

            // Get all pets owned by the user
            var petIds = await db.PetOwners
                .Where(po => po.UserProfileId == userProfile.Id)
                .Select(po => po.PetId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var start = startDate ?? now;
            var end = endDate ?? now.AddMonths(3);

            // Get upcoming appointments
            var upcomingAppointments = await db.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Where(a => petIds.Contains(a.PetId) &&
                       a.AppointmentDate >= start &&
                       a.AppointmentDate <= end &&
                       a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            // Get active medications with reminders
            var activeMedications = await db.Medications
                .Include(m => m.Pet)
                .Where(m => petIds.Contains(m.PetId) &&
                       m.IsActive &&
                       m.ReminderEnabled &&
                       m.NextReminderDue.HasValue &&
                       m.NextReminderDue >= start &&
                       m.NextReminderDue <= end)
                .OrderBy(m => m.NextReminderDue)
                .ToListAsync();

            // Get upcoming vaccinations
            var upcomingVaccinations = await db.HealthRecords
                .Include(hr => hr.Pet)
                .Include(hr => hr.Veterinarian)
                .Where(hr => petIds.Contains(hr.PetId) &&
                       hr.RecordType == "Vaccination" &&
                       hr.DueDate.HasValue &&
                       hr.DueDate >= start &&
                       hr.DueDate <= end)
                .OrderBy(hr => hr.DueDate)
                .ToListAsync();

            // Create calendar events from appointments, medications, and vaccinations
            var calendarEvents = new List<CalendarEventDto>();

            // Add appointments to calendar events
            foreach (var appointment in upcomingAppointments)
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = appointment.Id,
                    EventType = "Appointment",
                    Title = $"{appointment.AppointmentType} - {appointment.Pet.Name}",
                    Description = appointment.Notes ?? string.Empty,
                    EventDate = appointment.AppointmentDate,
                    EventTime = appointment.AppointmentTime,
                    PetId = appointment.PetId,
                    PetName = appointment.Pet.Name,
                    Color = "#4285F4" // Blue
                });
            }

            // Add medication reminders to calendar events
            foreach (var medication in activeMedications)
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = medication.Id,
                    EventType = "Medication",
                    Title = $"Medication: {medication.Name} - {medication.Pet.Name}",
                    Description = $"{medication.Dosage}, {medication.Instructions}",
                    EventDate = medication.NextReminderDue.Value.Date,
                    EventTime = medication.ReminderTime,
                    PetId = medication.PetId,
                    PetName = medication.Pet.Name,
                    Color = "#EA4335" // Red
                });
            }

            // Add vaccination due dates to calendar events
            foreach (var vaccination in upcomingVaccinations)
            {
                calendarEvents.Add(new CalendarEventDto
                {
                    Id = vaccination.Id,
                    EventType = "Vaccination",
                    Title = $"Vaccination Due: {vaccination.Description} - {vaccination.Pet.Name}",
                    Description = vaccination.Notes ?? string.Empty,
                    EventDate = vaccination.DueDate.Value,
                    EventTime = null,
                    PetId = vaccination.PetId,
                    PetName = vaccination.Pet.Name,
                    Color = "#FBBC05" // Yellow
                });
            }

            return Results.Ok(calendarEvents.OrderBy(e => e.EventDate).ThenBy(e => e.EventTime).ToList());
        }).RequireAuthorization();
    }
}