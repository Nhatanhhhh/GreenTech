# GreenTech Project

## 📋 Tổng quan

Dự án GreenTech là hệ thống thương mại điện tử được xây dựng bằng .NET 8 với kiến trúc 3 lớp:
- **DAL**: Data Access Layer (DbContext + Migrations)
- **BLL**: Business Logic Layer  
- **Presentation**: 2 ứng dụng web (Razor Pages + MVC)

## 🛠 Yêu cầu hệ thống

- .NET 8 SDK
- SQL Server (2019+)
- Visual Studio 2022 / VS Code

## 🚀 Cách chạy dự án

### Bước 1: Restore packages
```bash
dotnet restore
```

### Bước 2: Build solution
```bash
dotnet build
```

### Bước 3: Cấu hình Database

**Chỉnh sửa connection string trong `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=GreenTechDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### Bước 4: Tạo Database (Code First)

**Tạo migration:**
```powershell
Add-Migration InitialCreate -Project DAL -StartupProject GreenTech
```

**Cập nhật database:**
```powershell
Update-Database -Project DAL -StartupProject GreenTech
```

### Bước 5: Chạy ứng dụng

**Razor Pages:**
```bash
cd GreenTech
dotnet run
```

**MVC:**
```bash
cd GreenTechMVC
dotnet run
```

## 🔧 Xử lý lỗi thường gặp

### Lỗi 1: "Unable to resolve service for type 'DbContextOptions'"

**Nguyên nhân:** EF Tool không thể tạo DbContext lúc migration.

**Giải pháp:** Tạo `ApplicationDbContextFactory.cs` trong `DAL/Context/`:

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

### Lỗi 2: "Database is in single user mode"

**Nguyên nhân:** Database ở chế độ single user sau migration.

**Giải pháp:** Chạy SQL sau migration:
```sql
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

### Lỗi 3: "Cannot drop database because it is currently in use"

**Giải pháp:**
```sql
-- Kill connections và đặt multi-user
ALTER DATABASE GreenTechDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE GreenTechDB SET MULTI_USER;
```

## 📝 Development Workflow

### Thêm Entity mới:
1. Tạo model trong `DAL/Models/`
2. Thêm DbSet vào `ApplicationDbContext`
3. Tạo migration: `Add-Migration AddNewEntity -Project DAL -StartupProject GreenTech`
4. Cập nhật database: `Update-Database -Project DAL -StartupProject GreenTech`
5. **Quan trọng:** Chạy `ALTER DATABASE GreenTechDB SET MULTI_USER;`

### Thay đổi Entity:
1. Chỉnh sửa model
2. Tạo migration: `Add-Migration UpdateEntity -Project DAL -StartupProject GreenTech`
3. Cập nhật database: `Update-Database -Project DAL -StartupProject GreenTech`
4. Chạy `ALTER DATABASE GreenTechDB SET MULTI_USER;`

## 🎯 Lưu ý quan trọng

- **Luôn chạy** `ALTER DATABASE GreenTechDB SET MULTI_USER;` sau mỗi migration
- **Kiểm tra connection string** trước khi chạy migration
- **Backup database** trước khi thay đổi lớn
- **Sử dụng ApplicationDbContextFactory** để tránh lỗi DbContextOptions

## 📞 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra connection string
2. Đảm bảo SQL Server đang chạy
3. Kiểm tra database có ở multi-user mode không
4. Xem lại log files để debug