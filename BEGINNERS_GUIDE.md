# PetPal - Beginner's Guide

Welcome to PetPal! This guide is designed to help beginners understand the project and get it up and running on their local machine. We'll explain key concepts in simple terms and provide step-by-step instructions.

## What is PetPal?

PetPal is a digital system that helps pet owners keep track of everything related to their pets. Think of it as a digital pet management tool where you can store information about your pets, their health records, medications, and appointments.

## Key Concepts Explained

### Web API
A Web API (Application Programming Interface) is like a waiter in a restaurant. When you (the client/frontend) want something from the kitchen (the database), you don't go directly to the kitchen. Instead, you tell the waiter (the API) what you want, and they bring it to you. In PetPal, the API handles requests from the frontend, processes them, and returns the appropriate data.

### .NET Core
.NET Core is a framework (a collection of pre-written code) created by Microsoft that helps developers build applications more easily. It's like a toolbox that contains many useful tools for building software.

### Entity Framework Core
Entity Framework Core is an Object-Relational Mapper (ORM). That's a fancy way of saying it translates between the way data is stored in a database and the way it's used in code. It's like a translator that helps your C# code talk to the database.

### PostgreSQL
PostgreSQL is a type of database. A database is like a digital filing cabinet where all the information for the application is stored. PostgreSQL is particularly powerful and flexible, making it a good choice for applications like PetPal.

### Minimal API
Minimal API is a streamlined way to create web APIs in .NET. It uses less code and is more straightforward than traditional approaches, making it easier for beginners to understand.

## Step-by-Step Setup Guide

### 1. Install Required Software

#### Install .NET 7.0 SDK
1. Go to [https://dotnet.microsoft.com/download/dotnet/7.0](https://dotnet.microsoft.com/download/dotnet/7.0)
2. Download the installer for your operating system (Windows, macOS, or Linux)
3. Run the installer and follow the prompts
4. Verify the installation by opening a command prompt or terminal and typing:
   ```
   dotnet --version
   ```
   You should see a version number starting with 7.0.

#### Install PostgreSQL
1. Go to [https://www.postgresql.org/download/](https://www.postgresql.org/download/)
2. Select your operating system and follow the download instructions
3. During installation:
   - Remember the password you set for the 'postgres' user
   - The default port is 5432, it's recommended to keep this default
4. After installation, you can verify it's running by:
   - On Windows: Check Services for "postgresql-x64-xx"
   - On macOS/Linux: Run `ps aux | grep postgres`

#### Install Git
1. Go to [https://git-scm.com/downloads](https://git-scm.com/downloads)
2. Download the installer for your operating system
3. Run the installer with default options
4. Verify the installation by opening a command prompt or terminal and typing:
   ```
   git --version
   ```

#### Install Visual Studio Code (Recommended for beginners)
1. Go to [https://code.visualstudio.com/](https://code.visualstudio.com/)
2. Download the installer for your operating system
3. Run the installer
4. After installation, open VS Code and install the following extensions:
   - C# (by Microsoft)
   - C# Dev Kit (by Microsoft)
   - PostgreSQL (by Chris Kolkman)

### 2. Clone the Repository

1. Open a command prompt or terminal
2. Navigate to where you want to store the project:
   ```
   cd Documents/Projects  # Or any folder of your choice
   ```
3. Clone the repository:
   ```
   git clone https://github.com/yourusername/petpal.git
   ```
4. Navigate into the project folder:
   ```
   cd petpal/server
   ```

### 3. Set Up the Database

1. Open pgAdmin (comes with PostgreSQL) or another PostgreSQL client
2. Connect to your PostgreSQL server using the password you set during installation
3. Create a new database called 'petpal':
   - Right-click on 'Databases' and select 'Create' > 'Database'
   - Name it 'petpal' and save

4. Update the connection string:
   - Open the project in VS Code:
     ```
     code .
     ```
   - Find and open the file `PetPal.API/appsettings.json`
   - Update the connection string to match your PostgreSQL setup:
     ```json
     {
       "PetPalDbConnectionString": "Host=localhost;Database=petpal;Username=postgres;Password=yourpassword"
     }
     ```
     Replace 'yourpassword' with the password you set during PostgreSQL installation

5. Apply database migrations:
   - In the terminal, navigate to the API project:
     ```
     cd PetPal.API
     ```
   - Run the migrations:
     ```
     dotnet ef database update
     ```
   - This command creates all the necessary tables in your database

### 4. Run the Application

1. In the terminal, make sure you're in the PetPal.API directory
2. Run the application:
   ```
   dotnet run
   ```
3. You should see output indicating that the application has started, including URLs where the API is running (typically https://localhost:7000 and http://localhost:5000)
4. Open a web browser and navigate to one of these URLs
5. You should see a message: "PetPal API is running!"

### 5. Explore the API

Now that the API is running, you can explore it using a tool like Postman or the built-in HTTP file:

1. In VS Code, open the file `PetPal.API/PetPal.API.http`
2. This file contains example requests that you can send to the API
3. To send a request, click the "Send Request" link above each request

## Common Issues and Solutions

### Database Connection Failed
- Check that PostgreSQL is running
- Verify your connection string in appsettings.json
- Ensure the username and password are correct
- Make sure the 'petpal' database exists

### Migration Failed
- Make sure you're in the PetPal.API directory when running the migration command
- Check for any error messages in the output
- Try running `dotnet ef migrations add InitialCreate` first if no migrations exist

### API Won't Start
- Check if another application is using the same ports
- Make sure all required packages are installed with `dotnet restore`
- Check for error messages in the terminal output

## Next Steps

Now that you have the API running, you might want to:

1. Explore the code to understand how it works
2. Create a frontend application to interact with the API
3. Add new features to the API
4. Write tests for the existing functionality

## Learning Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core Minimal APIs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

Happy coding! 🐾