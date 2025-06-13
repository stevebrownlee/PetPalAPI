using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PetPal.API.DTOs;
using PetPal.API.Models;

namespace PetPal.API.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // User Profile mappings
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<RegistrationDto, UserProfile>();
        CreateMap<UpdateUserProfileDto, UserProfile>();

        // Pet mappings
        CreateMap<Pet, PetDto>();
        CreateMap<PetCreateDto, Pet>();
        CreateMap<PetUpdateDto, Pet>();

        // PetOwner mappings
        CreateMap<PetOwner, PetOwnerDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => $"{src.UserProfile.FirstName} {src.UserProfile.LastName}"));
        CreateMap<AddPetOwnerDto, PetOwner>();

        // Health Record mappings
        CreateMap<HealthRecord, HealthRecordDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<HealthRecordCreateDto, HealthRecord>();
        CreateMap<HealthRecordUpdateDto, HealthRecord>();

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}"));
        CreateMap<AppointmentCreateDto, Appointment>();
        CreateMap<AppointmentUpdateDto, Appointment>();

        // Medication mappings
        CreateMap<Medication, MedicationDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name));
        CreateMap<MedicationCreateDto, Medication>();
        CreateMap<MedicationUpdateDto, Medication>();

        // Veterinarian mappings
        CreateMap<Veterinarian, VeterinarianDto>();
        CreateMap<VeterinarianCreateDto, Veterinarian>();
        CreateMap<VeterinarianUpdateDto, Veterinarian>();

        // Care Provider mappings
        CreateMap<CareProvider, CareProviderDto>();
        CreateMap<CareProviderCreateDto, CareProvider>();
        CreateMap<CareProviderUpdateDto, CareProvider>();

        // Vaccination mappings (using HealthRecord with RecordType="Vaccination")
        CreateMap<HealthRecord, VaccinationDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<VaccinationCreateDto, HealthRecord>()
            .ForMember(dest => dest.RecordType, opt => opt.MapFrom(src => "Vaccination"));
        CreateMap<VaccinationUpdateDto, HealthRecord>();

        // Weight mappings
        CreateMap<Weight, WeightDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name));
        CreateMap<WeightCreateDto, Weight>();
        CreateMap<WeightUpdateDto, Weight>();
        CreateMap<Weight, WeightHistoryDto>();

        // Feeding Schedule mappings
        CreateMap<FeedingSchedule, FeedingScheduleDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name));
        CreateMap<FeedingScheduleCreateDto, FeedingSchedule>();
        CreateMap<FeedingScheduleUpdateDto, FeedingSchedule>();

        // Dashboard mappings
        CreateMap<Pet, PetDashboardDto>()
            .ForMember(dest => dest.CurrentWeight, opt => opt.MapFrom(src => src.Weight));
        CreateMap<Pet, PetDashboardSummaryDto>();
        CreateMap<HealthRecord, HealthRecordSummaryDto>()
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<Medication, MedicationSummaryDto>();
        CreateMap<Appointment, AppointmentSummaryDto>()
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<Weight, WeightSummaryDto>()
            .ForMember(dest => dest.RecordDate, opt => opt.MapFrom(src => src.Date));

        // Export mappings
        CreateMap<Pet, PetExportDto>();
        CreateMap<UserProfile, OwnerExportDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        CreateMap<HealthRecord, HealthRecordExportDto>()
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<Medication, MedicationExportDto>();
        CreateMap<Appointment, AppointmentExportDto>()
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src =>
                src.Veterinarian != null ? $"{src.Veterinarian.FirstName} {src.Veterinarian.LastName}" : null));
        CreateMap<Weight, WeightExportDto>()
            .ForMember(dest => dest.RecordDate, opt => opt.MapFrom(src => src.Date));
        CreateMap<FeedingSchedule, FeedingScheduleExportDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Portion))
            .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => $"{src.FeedingTime.Hours}:{src.FeedingTime.Minutes:D2}"))
            .ForMember(dest => dest.SpecialInstructions, opt => opt.MapFrom(src => src.Notes));

        // Settings mappings
        CreateMap<NotificationSettings, NotificationSettingsDto>();
        CreateMap<NotificationSettingsUpdateDto, NotificationSettings>();
        CreateMap<UserProfile, ThemePreferenceDto>();
        CreateMap<ThemePreferenceUpdateDto, UserProfile>();
    }
}