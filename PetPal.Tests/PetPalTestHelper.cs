using System.Text.Json;
using PetPal.API.Data;
using PetPal.API.DTOs;

namespace PetPal.Tests;

public static class TestHelper {
    // No static factory or client

    public static void SeedDatabase (PetPalDbContext dbContext) {
        // Create roles

        // Create identity users

        // Create user profiles

        // Create pets
    }

    // HTTP client methods for integration testing
    public static async Task<List<PetDto>> GetAllPetsAsync (HttpClient client) {
        var response = await client.GetAsync ("/scientists");
        response.EnsureSuccessStatusCode ();
        var content = await response.Content.ReadAsStringAsync ();
        List<PetDto>? pets = JsonSerializer.Deserialize<List<PetDto>> (content, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        if (pets == null) {
            throw new Exception ("Failed to deserialize pets");
        }
        return pets;
    }
}