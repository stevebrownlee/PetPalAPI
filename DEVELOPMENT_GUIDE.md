# PetPal Development Guide

This guide is intended for developers who want to contribute to the PetPal project or extend its functionality. It covers the project architecture, development workflow, and best practices.

## Project Architecture

PetPal follows a clean architecture approach with the following components:

### API Layer
- Uses ASP.NET Core Minimal API pattern
- Defined in the `Endpoints` folder
- Each endpoint group is in a separate file (e.g., `PetEndpoints.cs`, `AuthEndpoints.cs`)
- Handles HTTP requests, authentication, and authorization

### Data Transfer Objects (DTOs)
- Located in the `DTOs` folder
- Used for data validation and to control what data is exposed to clients
- Prevents exposing sensitive or unnecessary data
- Helps with versioning the API

### Domain Models
- Located in the `Models` folder
- Represent the core business entities (Pet, HealthRecord, etc.)
- Define relationships between entities
- Include business logic related to the entity

### Data Access Layer
- Uses Entity Framework Core with PostgreSQL
- `PetPalDbContext.cs` defines the database schema and relationships
- `DbInitializer.cs` handles database seeding

### Authentication and Authorization
- Uses ASP.NET Identity for user management
- Cookie-based authentication
- Role-based authorization (Admin, regular users)

## Development Workflow

### Setting Up the Development Environment

1. Clone the repository
2. Install the required tools (.NET 8.0 SDK, PostgreSQL)
3. Set up the database as described in the README.md
4. Open the solution in your preferred IDE (Visual Studio, VS Code, Rider)

### Making Changes

1. Create a new branch for your feature or bug fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes, following the coding standards and architecture

3. Run the application locally to test your changes. The cofniguration files are already in the project to start the program in debug mode.

4. Write or update tests for your changes:
   ```bash
   cd PetPal.Tests
   dotnet test
   ```

5. Commit your changes with a descriptive message:
   ```bash
   git commit -m "Add feature: your feature description"
   ```

6. Push your branch to the remote repository:
   ```bash
   git push origin feature/your-feature-name
   ```

7. Create a pull request for review

### Adding a New Entity

To add a new entity to the system (e.g., a new `Vaccination` entity):

1. Create a new model class in the `Models` folder:
   ```csharp
   // Models/Vaccination.cs
   namespace PetPal.API.Models;

   public class Vaccination
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public DateTime Date { get; set; }
       public DateTime ExpirationDate { get; set; }
       public string Notes { get; set; }
       public int PetId { get; set; }
       public Pet Pet { get; set; }
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
   }
   ```

2. Add the entity to the `PetPalDbContext.cs`:
   ```csharp
   public DbSet<Vaccination> Vaccinations { get; set; }
   ```

3. Configure the entity relationships in the `OnModelCreating` method:
   ```csharp
   modelBuilder.Entity<Vaccination>()
       .HasOne(v => v.Pet)
       .WithMany(p => p.Vaccinations)
       .HasForeignKey(v => v.PetId)
       .OnDelete(DeleteBehavior.Cascade);
   ```

4. Update the `Pet` model to include the relationship:
   ```csharp
   public List<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
   ```

5. Create DTOs for the entity in a new file `DTOs/VaccinationDtos.cs`:
   ```csharp
   namespace PetPal.API.DTOs;

   public class VaccinationDto
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public DateTime Date { get; set; }
       public DateTime ExpirationDate { get; set; }
       public string Notes { get; set; }
       public int PetId { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime UpdatedAt { get; set; }
   }

   public class VaccinationCreateDto
   {
       public string Name { get; set; }
       public DateTime Date { get; set; }
       public DateTime ExpirationDate { get; set; }
       public string Notes { get; set; }
   }

   public class VaccinationUpdateDto
   {
       public string Name { get; set; }
       public DateTime Date { get; set; }
       public DateTime ExpirationDate { get; set; }
       public string Notes { get; set; }
   }
   ```

6. Add mapping profiles in `Helpers/MappingProfiles.cs`:
   ```csharp
   CreateMap<Vaccination, VaccinationDto>();
   CreateMap<VaccinationCreateDto, Vaccination>();
   CreateMap<VaccinationUpdateDto, Vaccination>();
   ```

7. Create endpoints in a new file `Endpoints/VaccinationEndpoints.cs`:
   ```csharp
   namespace PetPal.API.Endpoints;

   public static class VaccinationEndpoints
   {
       public static void MapVaccinationEndpoints(this WebApplication app)
       {
           // Implement endpoints for CRUD operations
           // ...
       }
   }
   ```

8. Register the endpoints in `Program.cs`:
   ```csharp
   app.MapVaccinationEndpoints();
   ```

9. Create a migration to update the database:
   ```bash
   dotnet ef migrations add AddVaccinations
   dotnet ef database update
   ```

### Adding a New Endpoint

To add a new endpoint to an existing entity:

1. Open the appropriate endpoints file (e.g., `PetEndpoints.cs`)
2. Add your new endpoint:
   ```csharp
   // Example: Get pets by species
   app.MapGet("/pets/species/{species}", async (
       string species,
       PetPalDbContext db,
       IMapper mapper) =>
   {
       var pets = await db.Pets
           .Where(p => p.Species.ToLower() == species.ToLower())
           .Include(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
           .ThenInclude(po => po.UserProfile)
           .ToListAsync();

       return Results.Ok(mapper.Map<List<PetDto>>(pets));
   }).RequireAuthorization("AdminOnly");
   ```

3. Test the endpoint to ensure it works as expected

## Best Practices

### Code Style

- Follow C# naming conventions:
  - PascalCase for class names, properties, and methods
  - camelCase for local variables and parameters
- Use meaningful names that describe the purpose
- Keep methods small and focused on a single responsibility
- Use comments to explain complex logic or business rules

### API Design

- Use appropriate HTTP methods (GET, POST, PUT, DELETE)
- Return appropriate status codes
- Use consistent URL patterns
- Validate input data
- Include pagination for endpoints that return collections
- Use DTOs to control what data is exposed

### Security

- Always validate user input
- Use authorization attributes to protect endpoints
- Don't expose sensitive information in responses
- Use HTTPS in production
- Follow the principle of least privilege

### Database

- Use migrations for all database changes
- Include indexes for frequently queried fields
- Be mindful of query performance
- Use appropriate cascade delete behaviors
- Consider using transactions for operations that modify multiple entities

### Testing

- Write unit tests for business logic
- Write integration tests for endpoints
- Use a test database for integration tests
- Mock external dependencies

## Common Tasks

### Adding a New Role

1. Update the authorization policies in `Program.cs`:
   ```csharp
   builder.Services.AddAuthorization(options =>
   {
       // Existing policies...

       // Add a new policy for the new role
       options.AddPolicy("StaffAccess", policy => policy.RequireRole("Admin", "Staff"));
   });
   ```

2. Seed the role in `DbInitializer.cs`:
   ```csharp
   if (!await roleManager.RoleExistsAsync("Staff"))
   {
       await roleManager.CreateAsync(new IdentityRole("Staff"));
   }
   ```

3. Use the policy in your endpoints:
   ```csharp
   app.MapGet("/some-endpoint", async (/* parameters */) =>
   {
       // Implementation
   }).RequireAuthorization("StaffAccess");
   ```

### Adding Filtering and Pagination

For endpoints that return collections, add filtering and pagination:

```csharp
app.MapGet("/pets", async (
    [FromQuery] string species,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    PetPalDbContext db,
    IMapper mapper) =>
{
    var query = db.Pets.AsQueryable();

    // Apply filters if provided
    if (!string.IsNullOrEmpty(species))
    {
        query = query.Where(p => p.Species.ToLower() == species.ToLower());
    }

    // Apply pagination
    var totalCount = await query.CountAsync();
    var pets = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Include(p => p.Owners.Where(o => o.PetId == o.Pet.Id))
        .ThenInclude(po => po.UserProfile)
        .ToListAsync();

    // Return with pagination metadata
    return Results.Ok(new {
        items = mapper.Map<List<PetDto>>(pets),
        totalCount,
        page,
        pageSize,
        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    });
}).RequireAuthorization("AdminOnly");
```

## Troubleshooting

### Common Issues

#### Entity Framework Migrations

If you encounter issues with migrations:

1. Delete the `Migrations` folder
2. Create a new initial migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```
3. Update the database:
   ```bash
   dotnet ef database update
   ```

#### Authentication Issues

If authentication is not working:

1. Check that the cookie settings in `Program.cs` are correct
2. Ensure CORS is configured correctly if testing from a different origin
3. Verify that the user exists and the password is correct
4. Check that the roles are assigned correctly

#### Database Connection Issues

If you can't connect to the database:

1. Verify that PostgreSQL is running
2. Check the connection string in `appsettings.json`
3. Ensure the database exists
4. Check that the user has the necessary permissions

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)
- [Minimal API Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [AutoMapper Documentation](https://docs.automapper.org)