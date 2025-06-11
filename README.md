# PetPal - Pet Management System

PetPal is a comprehensive pet management system designed to help pet owners keep track of their pets' information, health records, medications, appointments, and more. This application provides a user-friendly interface for managing all aspects of pet care.

## 🐾 Project Overview

PetPal allows pet owners to store and manage detailed information about their pets, including:
- Basic pet details (name, species, breed, etc.)
- Health records and veterinary visits
- Medication tracking
- Appointment scheduling
- Multi-owner pet management

The system is built with a .NET Core backend API and is designed to be used with a frontend client application (not included in this repository).

## ✨ Features

- **User Authentication**: Secure registration and login system
- **Pet Management**: Add, view, update, and delete pets
- **Multi-Owner Support**: Share pet profiles with family members or co-owners
- **Health Records**: Track veterinary visits and health information
- **Medication Management**: Record and track pet medications
- **Appointment Scheduling**: Manage veterinary and other pet-related appointments
- **Role-Based Access Control**: Admin and regular user roles with appropriate permissions

## 🛠️ Technologies Used

- **Backend**: .NET 7.0, ASP.NET Core
- **API Style**: Minimal API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Identity with cookie authentication
- **Object Mapping**: AutoMapper
- **Data Format**: JSON

## 📋 Prerequisites

Before you begin, ensure you have the following installed:
- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/downloads)
- A code editor (recommended: [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/))

## 🚀 Getting Started

### Cloning the Repository

```bash
# Clone the repository
git clone https://github.com/yourusername/petpal.git

# Navigate to the project directory
cd petpal/server
```

### Setting Up the Database

1. Create a PostgreSQL database for the project:
```sql
CREATE DATABASE petpal;
```

2. Create a connection string user secret
   ```sh
   dotnet user-secrets init
   dotnet user-secrets set 'PetPalDbConnectionString' 'Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=PetPal'
   ```

3. Apply the database migrations:
   ```sh
   cd PetPal.API
   dotnet ef database update
   ```

### Running the API

There is a `launch.json` and `tasks.json` file already in the repostitory, so you can immediately start the program in debug mode.

1. The API will be available at `http://localhost:5000`

## 📡 API Endpoints

The API provides the following main endpoint groups:

### Authentication
- `POST /auth/register` - Register a new user
- `POST /auth/login` - Log in a user
- `POST /auth/logout` - Log out a user

### Pets
- `GET /user/pets` - Get all pets for the current user
- `GET /pets/{id}` - Get a specific pet by ID
- `POST /pets` - Create a new pet
- `PUT /pets/{id}` - Update a pet
- `DELETE /pets/{id}` - Delete a pet
- `POST /pets/{petId}/owners` - Add an owner to a pet
- `DELETE /pets/{petId}/owners/{ownerId}` - Remove an owner from a pet

### Health Records
- `GET /pets/{petId}/healthrecords` - Get all health records for a pet
- `GET /healthrecords/{id}` - Get a specific health record
- `POST /pets/{petId}/healthrecords` - Create a new health record
- `PUT /healthrecords/{id}` - Update a health record
- `DELETE /healthrecords/{id}` - Delete a health record

### Appointments
- `GET /pets/{petId}/appointments` - Get all appointments for a pet
- `GET /appointments/{id}` - Get a specific appointment
- `POST /pets/{petId}/appointments` - Create a new appointment
- `PUT /appointments/{id}` - Update an appointment
- `DELETE /appointments/{id}` - Delete an appointment

## 📁 Project Structure

```
PetPal/
├── PetPal.API/              # Main API project
│   ├── Data/                # Database context and initialization
│   ├── DTOs/                # Data Transfer Objects
│   ├── Endpoints/           # API endpoint definitions
│   ├── Helpers/             # Helper classes
│   ├── Migrations/          # EF Core migrations
│   ├── Models/              # Domain models
│   └── Properties/          # Project properties
├── PetPal.Tests/            # Test project
└── PetPal.sln               # Solution file
```

## 🧩 Key Concepts for Beginners

### What is an API?
An API (Application Programming Interface) is a set of rules that allows different software applications to communicate with each other. In this project, the API serves as the backend that handles data storage, business logic, and authentication.

### What is ASP.NET Core?
ASP.NET Core is a cross-platform, high-performance framework for building modern, cloud-based, internet-connected applications. It's used to create the web API in this project.

### What is Entity Framework Core?
Entity Framework Core is an Object-Relational Mapping (ORM) framework that enables .NET developers to work with a database using .NET objects, eliminating the need for most of the data-access code that typically needs to be written.

### What is a Minimal API?
Minimal APIs are a simplified approach to building HTTP APIs with ASP.NET Core. They use a more concise syntax than traditional controller-based APIs, making them ideal for smaller applications or microservices.

### What is PostgreSQL?
PostgreSQL is a powerful, open-source object-relational database system that uses and extends the SQL language. It's known for its reliability, feature robustness, and performance.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.