# GreenTech Project

## 📋 Overview

GreenTech is an e-commerce system built with .NET 8 using a 3-tier architecture:
- **DAL**: Data Access Layer (DbContext + Migrations)
- **BLL**: Business Logic Layer  
- **Presentation**: 2 web applications (Razor Pages + MVC)

## 🛠 System Requirements

- .NET 8 SDK or higher
- SQL Server (2019+) or SQL Server Express
- Visual Studio 2022 / VS Code
- Node.js (for Tailwind CSS in MVC project)

## 🚀 How to Run the Project

### Step 1: Clone and Setup

```bash
# Clone the repository
git clone <repository-url>
cd GreenTech

# Restore .NET packages
dotnet restore

# Restore Node.js packages (for MVC project)
cd GreenTechMVC
npm install
cd ..
```

### Step 2: Configure Environment Variables

**Create `.env` file in the root directory of each project:**

**GreenTech/.env:**
```env
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**GreenTechMVC/.env:**
```env
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**⚠️ Important:** 
- Add `.env` to your `.gitignore` file to avoid committing sensitive data
- Use double underscores `__` to represent nested configuration structure
- The format is: `SECTION__KEY=value`

**Example .gitignore:**
```gitignore
*.env
.env
.env.*
```

### Step 3: Build the Solution

```bash
# Build entire solution
dotnet build

# Or build individual projects
dotnet build DAL
dotnet build BLL
dotnet build GreenTech
dotnet build GreenTechMVC
```

### Step 4: Setup Database (Code First)

**Create ApplicationDbContextFactory (Required for migrations):**

Create `DAL/Context/ApplicationDbContextFactory.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace DAL.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Load .env file from solution root
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }
            
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Not found CONNECTIONSTRINGS__DEFAULTCONNECTION in .env file");
            }

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
```

**Install required NuGet package:**
```bash
# Install DotNetEnv package for DAL project
cd DAL
dotnet add package DotNetEnv
cd ..
```

**Create and apply migrations:**

```powershell
# Create initial migration
Add-Migration InitialCreate -Project DAL -StartupProject GreenTech

# Update database
Update-Database -Project DAL -StartupProject GreenTech
```

**Alternative using Terminal:**
```bash
# Create migration
dotnet ef migrations add InitialCreate --project DAL --startup-project GreenTech

# Update database
dotnet ef database update --project DAL --startup-project GreenTech
```

### Database Management Commands

**Drop database and reset:**
```bash
# Drop database (force, will lose all data)
dotnet ef database drop --force --project DAL --startup-project GreenTech

# Remove last migration (before dropping database)
dotnet ef migrations remove --project DAL --startup-project GreenTech

# Remove specific migration
dotnet ef migrations remove --project DAL --startup-project GreenTech

# List all migrations
dotnet ef migrations list --project DAL --startup-project GreenTech
```

**Complete reset workflow:**
```bash
# 1. Drop database (lose all data)
dotnet ef database drop --force --project DAL --startup-project GreenTech

# 2. Remove all migrations (start fresh)
# Run multiple times until all migrations are removed
dotnet ef migrations remove --project DAL --startup-project GreenTech

# 3. Create new migration
dotnet ef migrations add InitialCreate --project DAL --startup-project GreenTech

# 4. Update database (seed data will be applied)
dotnet ef database update --project DAL --startup-project GreenTech
```

### Step 5: Run Applications

**Run Razor Pages Application:**
```bash
cd GreenTech
dotnet run
```
Application will be available at: `https://localhost:7xxx` or `http://localhost:5xxx`

**Run MVC Application:**
```bash
cd GreenTechMVC
dotnet run
```
Application will be available at: `https://localhost:7xxx` or `http://localhost:5xxx`

## 🔧 Troubleshooting Common Issues

### Issue 1: "Not found CONNECTIONSTRINGS__DEFAULTCONNECTION in .env"

**Cause:** .env file is missing or not loaded correctly.

**Solution:** 
- Ensure `.env` file exists in project root directory
- Check the variable name uses double underscores: `CONNECTIONSTRINGS__DEFAULTCONNECTION`
- Verify file encoding is UTF-8 without BOM

### Issue 2: "Unable to resolve service for type 'DbContextOptions'"

**Cause:** EF Tool cannot create DbContext instance during design-time.

**Solution:** 
- Create `ApplicationDbContextFactory.cs` as shown in Step 4
- Install `DotNetEnv` package in DAL project
- Ensure .env file is accessible from DAL project directory

### Issue 3: "Database is in single user mode"

**Cause:** Database is in single user mode after migration.

**Solution:** Run this SQL after migration:
```sql
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

### Issue 4: "Cannot drop database because it is currently in use"

**Solution:**
```sql
-- Kill connections and set multi-user
ALTER DATABASE GreenTechDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

### Issue 5: "TrustServerCertificate" Error

**Solution:** Add `TrustServerCertificate=True` to connection string in `.env`:
```env
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

### Issue 6: "Login failed"

**Check:**
- Username and password are correct in `.env` file
- SQL Server is running
- SQL Server Authentication is enabled
- User has permission to create database

## 📝 Development Workflow

### Adding New Entity:
1. Create model in `DAL/Models/`
2. Add DbSet to `AppDbContext`
3. Create migration: `dotnet ef migrations add AddNewEntity --project DAL --startup-project GreenTech`
4. Update database: `dotnet ef database update --project DAL --startup-project GreenTech`
5. **Important:** Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Modifying Entity:
1. Edit model
2. Create migration: `dotnet ef migrations add UpdateEntity --project DAL --startup-project GreenTech`
3. Update database: `dotnet ef database update --project DAL --startup-project GreenTech`
4. Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Removing Entity:
1. Remove DbSet from DbContext
2. Create migration: `dotnet ef migrations add RemoveEntity --project DAL --startup-project GreenTech`
3. Update database: `dotnet ef database update --project DAL --startup-project GreenTech`
4. Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Resetting Database (When Seed Data Changed):
```bash
# 1. Drop database (lose all data)
dotnet ef database drop --force --project DAL --startup-project GreenTech

# 2. Remove all migrations
dotnet ef migrations remove --project DAL --startup-project GreenTech

# 3. Create new migration with updated seed data
dotnet ef migrations add InitialCreate --project DAL --startup-project GreenTech

# 4. Update database with new seed data
dotnet ef database update --project DAL --startup-project GreenTech
```

## 🎯 Important Notes

- **Always add `.env` to `.gitignore`** to protect sensitive data
- **Use double underscores** `__` in environment variable names for nested configuration
- **Always run** `ALTER DATABASE GreenTechDB SET MULTI_USER;` after each migration
- **Check .env file** before running migrations
- **Backup database** before major changes
- **Use ApplicationDbContextFactory** to avoid DbContextOptions errors
- **Install DotNetEnv package** in all projects that need to read .env file
- **Node.js packages** are required for MVC project (Tailwind CSS)

## 🏗️ Project Structure
```
GreenTech/
├── DAL/                    # Data Access Layer (DbContext + Migrations)
├── BLL/                    # Business Logic Layer
├── GreenTech/              # Razor Pages Application (Startup Project)
│   └── .env               # Environment variables (DO NOT COMMIT)
├── GreenTechMVC/           # MVC Application (Startup Project)
│   └── .env               # Environment variables (DO NOT COMMIT)
├── .gitignore             # Include *.env here
└── README.md
```

## 📦 Required NuGet Packages

All projects using .env need:
```bash
dotnet add package DotNetEnv
```

## 🔒 Security Best Practices

1. **Never commit .env files** to version control
2. Add `.env` to `.gitignore`
3. Create `.env.example` with dummy values as template:
```env
   CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=localhost;Database=GreenTechDB;User Id=your_user;Password=your_password;TrustServerCertificate=True;
```
4. Share `.env.example` with team members, not actual `.env` file
5. Use different `.env` files for different environments (dev, staging, production)

## 📞 Support

If you encounter issues:
1. Check `.env` file exists and has correct variable names
2. Verify connection string format in `.env`
3. Ensure SQL Server is running
4. Verify database is in multi-user mode
5. Check log files for debugging
6. Ensure `ApplicationDbContextFactory` exists if getting DbContextOptions errors
7. Verify `DotNetEnv` package is installed

## 📚 References

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [DotNetEnv Package](https://github.com/tonerdo/dotnet-env)