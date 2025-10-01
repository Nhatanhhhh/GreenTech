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

### Step 2: Configure Database Connection

**Edit connection string in both projects:**

**GreenTech/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

**GreenTechMVC/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
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

namespace DAL.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=GreenTechDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
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

### Issue 1: "Unable to resolve service for type 'DbContextOptions'"

**Cause:** EF Tool cannot create DbContext instance during design-time.

**Solution:** Create `ApplicationDbContextFactory.cs` as shown in Step 4 above.

### Issue 2: "Database is in single user mode"

**Cause:** Database is in single user mode after migration.

**Solution:** Run this SQL after migration:
```sql
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

### Issue 3: "Cannot drop database because it is currently in use"

**Solution:**
```sql
-- Kill connections and set multi-user
ALTER DATABASE GreenTechDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

### Issue 4: "TrustServerCertificate" Error

**Solution:** Add `TrustServerCertificate=True` to connection string:
```
Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

### Issue 5: "Login failed"

**Check:**
- Username and password are correct
- SQL Server is running
- SQL Server Authentication is enabled
- User has permission to create database

## 📝 Development Workflow

### Adding New Entity:
1. Create model in `DAL/Models/`
2. Add DbSet to `ApplicationDbContext`
3. Create migration: `Add-Migration AddNewEntity -Project DAL -StartupProject GreenTech`
4. Update database: `Update-Database -Project DAL -StartupProject GreenTech`
5. **Important:** Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Modifying Entity:
1. Edit model
2. Create migration: `Add-Migration UpdateEntity -Project DAL -StartupProject GreenTech`
3. Update database: `Update-Database -Project DAL -StartupProject GreenTech`
4. Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Removing Entity:
1. Remove DbSet from DbContext
2. Create migration: `Add-Migration RemoveEntity -Project DAL -StartupProject GreenTech`
3. Update database: `Update-Database -Project DAL -StartupProject GreenTech`
4. Run `ALTER DATABASE GreenTechDB SET MULTI_USER;`

## 🎯 Important Notes

- **Always run** `ALTER DATABASE GreenTechDB SET MULTI_USER;` after each migration
- **Check connection string** before running migrations
- **Backup database** before major changes
- **Use ApplicationDbContextFactory** to avoid DbContextOptions errors
- **Node.js packages** are required for MVC project (Tailwind CSS)

## 🏗️ Project Structure

```
GreenTech/
├── DAL/                    # Data Access Layer (DbContext + Migrations)
├── BLL/                    # Business Logic Layer
├── GreenTech/              # Razor Pages Application (Startup Project)
├── GreenTechMVC/           # MVC Application (Startup Project)
└── README.md
```

## 📞 Support

If you encounter issues:
1. Check connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Verify database is in multi-user mode
4. Check log files for debugging
5. Ensure `ApplicationDbContextFactory` exists if getting DbContextOptions errors

## 📚 References

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)