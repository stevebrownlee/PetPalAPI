using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPal.API.Data;
using PetPal.API.DTOs;
using PetPal.API.Models;
using System.Security.Claims;
using System.Text;

namespace PetPal.API.Endpoints;

public static class ExportEndpoints
{
    public static void MapExportEndpoints(this WebApplication app)
    {
        // Export pet records
        app.MapPost("/pets/{petId}/export", async (
            int petId,
            [FromBody] ExportRequestDto exportRequest,
            ClaimsPrincipal user,
            PetPalDbContext db,
            IMapper mapper,
            HttpContext httpContext) =>
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

            // Validate export format
            if (string.IsNullOrEmpty(exportRequest.Format) ||
                (exportRequest.Format != "PDF" && exportRequest.Format != "CSV"))
            {
                return Results.BadRequest("Invalid export format. Supported formats: PDF, CSV");
            }

            // Set default sections if none provided
            if (exportRequest.Sections == null || exportRequest.Sections.Count == 0)
            {
                exportRequest.Sections = new List<string> { "All" };
            }

            // Prepare date range filter
            var startDate = exportRequest.StartDate ?? DateTime.MinValue;
            var endDate = exportRequest.EndDate ?? DateTime.MaxValue;

            // Create the export DTO
            var exportDto = new PetExportDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                DateOfBirth = pet.DateOfBirth,
                Weight = pet.Weight,
                Color = pet.Color,
                MicrochipNumber = pet.MicrochipNumber,
                ExportFormat = exportRequest.Format
            };

            // Load owner information
            if (exportRequest.Sections.Contains("BasicInfo") || exportRequest.Sections.Contains("All"))
            {
                var owners = await db.PetOwners
                    .Include(po => po.UserProfile)
                    .Where(po => po.PetId == petId)
                    .ToListAsync();

                foreach (var owner in owners)
                {
                    exportDto.Owners.Add(new OwnerExportDto
                    {
                        Name = $"{owner.UserProfile.FirstName} {owner.UserProfile.LastName}",
                        Email = owner.UserProfile.Email,
                        Phone = owner.UserProfile.Phone
                    });
                }
            }

            // Load health records
            if (exportRequest.Sections.Contains("HealthRecords") || exportRequest.Sections.Contains("All"))
            {
                var healthRecords = await db.HealthRecords
                    .Include(hr => hr.Veterinarian)
                    .Where(hr => hr.PetId == petId && hr.RecordDate >= startDate && hr.RecordDate <= endDate)
                    .OrderByDescending(hr => hr.RecordDate)
                    .ToListAsync();

                foreach (var record in healthRecords)
                {
                    exportDto.HealthRecords.Add(new HealthRecordExportDto
                    {
                        RecordType = record.RecordType,
                        Description = record.Description,
                        RecordDate = record.RecordDate,
                        DueDate = record.DueDate,
                        VeterinarianName = record.Veterinarian != null ? $"{record.Veterinarian.FirstName} {record.Veterinarian.LastName}" : null,
                        Notes = record.Notes
                    });
                }
            }

            // Load medications
            if (exportRequest.Sections.Contains("Medications") || exportRequest.Sections.Contains("All"))
            {
                var medications = await db.Medications
                    .Where(m => m.PetId == petId && m.StartDate >= startDate &&
                           (m.EndDate == null || m.EndDate <= endDate))
                    .OrderByDescending(m => m.StartDate)
                    .ToListAsync();

                foreach (var medication in medications)
                {
                    exportDto.Medications.Add(new MedicationExportDto
                    {
                        Name = medication.Name,
                        Dosage = medication.Dosage,
                        Frequency = medication.Frequency,
                        StartDate = medication.StartDate,
                        EndDate = medication.EndDate,
                        Instructions = medication.Instructions,
                        Prescriber = medication.Prescriber,
                        IsActive = medication.IsActive
                    });
                }
            }

            // Load appointments
            if (exportRequest.Sections.Contains("Appointments") || exportRequest.Sections.Contains("All"))
            {
                var appointments = await db.Appointments
                    .Include(a => a.Veterinarian)
                    .Where(a => a.PetId == petId && a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();

                foreach (var appointment in appointments)
                {
                    exportDto.Appointments.Add(new AppointmentExportDto
                    {
                        AppointmentDate = appointment.AppointmentDate,
                        AppointmentTime = appointment.AppointmentTime,
                        AppointmentType = appointment.AppointmentType,
                        VeterinarianName = appointment.Veterinarian != null ? $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}" : null,
                        Notes = appointment.Notes,
                        Status = appointment.Status
                    });
                }
            }

            // Load weight records
            if (exportRequest.Sections.Contains("WeightRecords") || exportRequest.Sections.Contains("All"))
            {
                var weightRecords = await db.Weights
                    .Where(w => w.PetId == petId && w.Date >= startDate && w.Date <= endDate)
                    .OrderByDescending(w => w.Date)
                    .ToListAsync();

                foreach (var weight in weightRecords)
                {
                    exportDto.WeightRecords.Add(new WeightExportDto
                    {
                        WeightValue = weight.WeightValue,
                        RecordDate = weight.Date,
                        Notes = weight.Notes
                    });
                }
            }

            // Load feeding schedules
            if (exportRequest.Sections.Contains("FeedingSchedules") || exportRequest.Sections.Contains("All"))
            {
                var feedingSchedules = await db.FeedingSchedules
                    .Where(fs => fs.PetId == petId)
                    .ToListAsync();

                foreach (var schedule in feedingSchedules)
                {
                    exportDto.FeedingSchedules.Add(new FeedingScheduleExportDto
                    {
                        FoodType = schedule.FoodType,
                        Amount = schedule.Portion,
                        Frequency = $"{schedule.FeedingTime.Hours}:{schedule.FeedingTime.Minutes:D2}",
                        SpecialInstructions = schedule.Notes
                    });
                }
            }

            // Generate the export file based on the format
            var result = new ExportResultDto
            {
                Success = true,
                Format = exportRequest.Format
            };

            if (exportRequest.Format == "PDF")
            {
                // Generate PDF file
                result = await GeneratePdfExport(exportDto, httpContext);
            }
            else if (exportRequest.Format == "CSV")
            {
                // Generate CSV file
                result = await GenerateCsvExport(exportDto, httpContext);
            }

            return Results.Ok(result);
        }).RequireAuthorization();
    }

    // Helper method to generate PDF export
    private static async Task<ExportResultDto> GeneratePdfExport(PetExportDto exportDto, HttpContext httpContext)
    {
        // TODO: Implement PDF generation using a library like iText or DinkToPdf
        // For now, we'll return a placeholder result

        // Create a unique filename
        var fileName = $"pet_{exportDto.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine("wwwroot", "exports", fileName);

        // Ensure the exports directory exists
        var exportsDir = Path.Combine(httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath, "exports");
        if (!Directory.Exists(exportsDir))
        {
            Directory.CreateDirectory(exportsDir);
        }

        // Placeholder for PDF generation
        // In a real implementation, this would use a PDF library to generate the file
        var placeholderText = $"PDF Export for {exportDto.Name} generated on {DateTime.UtcNow}";
        await File.WriteAllTextAsync(Path.Combine(exportsDir, fileName), placeholderText);

        return new ExportResultDto
        {
            Success = true,
            FileUrl = $"/exports/{fileName}",
            FileName = fileName,
            Format = "PDF"
        };
    }

    // Helper method to generate CSV export
    private static async Task<ExportResultDto> GenerateCsvExport(PetExportDto exportDto, HttpContext httpContext)
    {
        // Create a unique filename
        var fileName = $"pet_{exportDto.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var filePath = Path.Combine("wwwroot", "exports", fileName);

        // Ensure the exports directory exists
        var exportsDir = Path.Combine(httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath, "exports");
        if (!Directory.Exists(exportsDir))
        {
            Directory.CreateDirectory(exportsDir);
        }

        // Generate CSV content
        var csv = new StringBuilder();

        // Add pet basic information
        csv.AppendLine("PET INFORMATION");
        csv.AppendLine("Name,Species,Breed,Date of Birth,Weight,Color,Microchip Number");
        csv.AppendLine($"{exportDto.Name},{exportDto.Species},{exportDto.Breed},{exportDto.DateOfBirth:yyyy-MM-dd},{exportDto.Weight},{exportDto.Color},{exportDto.MicrochipNumber}");
        csv.AppendLine();

        // Add owner information
        if (exportDto.Owners.Any())
        {
            csv.AppendLine("OWNERS");
            csv.AppendLine("Name,Email,Phone");
            foreach (var owner in exportDto.Owners)
            {
                csv.AppendLine($"{owner.Name},{owner.Email},{owner.Phone}");
            }
            csv.AppendLine();
        }

        // Add health records
        if (exportDto.HealthRecords.Any())
        {
            csv.AppendLine("HEALTH RECORDS");
            csv.AppendLine("Record Type,Description,Record Date,Due Date,Veterinarian,Notes");
            foreach (var record in exportDto.HealthRecords)
            {
                csv.AppendLine($"{record.RecordType},{EscapeCsvField(record.Description)},{record.RecordDate:yyyy-MM-dd},{(record.DueDate.HasValue ? record.DueDate.Value.ToString("yyyy-MM-dd") : "")},{record.VeterinarianName},{EscapeCsvField(record.Notes)}");
            }
            csv.AppendLine();
        }

        // Add medications
        if (exportDto.Medications.Any())
        {
            csv.AppendLine("MEDICATIONS");
            csv.AppendLine("Name,Dosage,Frequency,Start Date,End Date,Instructions,Prescriber,Active");
            foreach (var medication in exportDto.Medications)
            {
                csv.AppendLine($"{medication.Name},{medication.Dosage},{medication.Frequency},{medication.StartDate:yyyy-MM-dd},{(medication.EndDate.HasValue ? medication.EndDate.Value.ToString("yyyy-MM-dd") : "")},{EscapeCsvField(medication.Instructions)},{medication.Prescriber},{medication.IsActive}");
            }
            csv.AppendLine();
        }

        // Add appointments
        if (exportDto.Appointments.Any())
        {
            csv.AppendLine("APPOINTMENTS");
            csv.AppendLine("Date,Time,Type,Veterinarian,Notes,Status");
            foreach (var appointment in exportDto.Appointments)
            {
                csv.AppendLine($"{appointment.AppointmentDate:yyyy-MM-dd},{appointment.AppointmentTime},{appointment.AppointmentType},{appointment.VeterinarianName},{EscapeCsvField(appointment.Notes)},{appointment.Status}");
            }
            csv.AppendLine();
        }

        // Add weight records
        if (exportDto.WeightRecords.Any())
        {
            csv.AppendLine("WEIGHT RECORDS");
            csv.AppendLine("Weight,Date,Notes");
            foreach (var weight in exportDto.WeightRecords)
            {
                csv.AppendLine($"{weight.WeightValue},{weight.RecordDate:yyyy-MM-dd},{EscapeCsvField(weight.Notes)}");
            }
            csv.AppendLine();
        }

        // Add feeding schedules
        if (exportDto.FeedingSchedules.Any())
        {
            csv.AppendLine("FEEDING SCHEDULES");
            csv.AppendLine("Food Type,Amount,Frequency,Special Instructions");
            foreach (var schedule in exportDto.FeedingSchedules)
            {
                csv.AppendLine($"{schedule.FoodType},{schedule.Amount},{schedule.Frequency},{EscapeCsvField(schedule.SpecialInstructions)}");
            }
        }

        // Write the CSV file
        await File.WriteAllTextAsync(Path.Combine(exportsDir, fileName), csv.ToString());

        return new ExportResultDto
        {
            Success = true,
            FileUrl = $"/exports/{fileName}",
            FileName = fileName,
            Format = "CSV"
        };
    }

    // Helper method to escape CSV fields
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        // If the field contains a comma, quote, or newline, wrap it in quotes and escape any quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}