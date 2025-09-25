using System.Net.Http.Json;
using PetPal.API.DTOs;

namespace PetPal.Tests;

public class PetTests : IClassFixture<PetPalWebApplicationFactory>, IDisposable
{
    private readonly PetPalWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient _authenticated_client = null!;

    protected async Task<HttpClient> GetAuthenticatedClientAsync(LoginDto credentials)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Login and get authentication cookie
        var loginData = new LoginDto
        {
            Email = credentials.Email,
            Password = credentials.Password
        };

        var loginResponse = await client.PostAsJsonAsync("/login", loginData);

        // Extract cookies from login response
        if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);
            }
        }

        return client;
    }

    public PetTests(PetPalWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllPets_ReturnsAllPets()
    {
        // Arrange
        _authenticated_client = await GetAuthenticatedClientAsync(new LoginDto
        {
            Email = "admin@petpal.com",
            Password = "Admin123!"
        });

        // Act
        var pets = await TestHelper.GetAllPetsAsync(_authenticated_client);

        // Assert we got a valid JSON response
        Assert.NotNull(pets);

        // Assert we have the expected number of pets (4 seeded in TestHelper)

        // Assert we have the expected pets by name
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}