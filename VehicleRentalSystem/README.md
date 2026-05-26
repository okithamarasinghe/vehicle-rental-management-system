# 🚗 VehicleRent Pro
### Enterprise Vehicle Rental Management System
**Built with:** C# · .NET 8 · ASP.NET Core MVC · Entity Framework Core · SQL Server · Bootstrap 5

---

## ⚡ Quick Start (5 minutes)

### Prerequisites
| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server / LocalDB | 2019+ | Included with Visual Studio |
| Visual Studio | 2022 | https://visualstudio.microsoft.com/ |

### Steps

```bash
# 1. Open the solution in Visual Studio 2022
#    File → Open → VehicleRentalSystem.sln

# 2. Update connection string (optional – LocalDB works out of the box)
#    Edit appsettings.json → ConnectionStrings → DefaultConnection

# 3. Run the application (F5 or Ctrl+F5)
#    Migrations and seed data are applied AUTOMATICALLY on first run.

# 4. Login with the default admin account:
#    URL:      https://localhost:5001
#    Email:    admin@vrms.lk
#    Password: Admin@12345
```

> **First thing to do:** Register a Staff user via Admin → User Management, then change the Admin password.

---

## 🏗 Architecture Overview

```
Presentation   →   Business Logic   →   Data Access   →   SQL Server
(Controllers,       (Services,           (Repositories,     (EF Core,
 Razor Views,        Business Rules,       LINQ Queries,      Migrations)
 ViewModels)         Validation)           Async/Await)
```

## 📁 Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | DI registration, middleware, Identity, DB seeding |
| `Data/ApplicationDbContext.cs` | EF Core DbContext with Fluent API & global filters |
| `Interfaces/IRepositories.cs` | Repository contracts |
| `Interfaces/IServices.cs` | Service contracts + `ServiceResult<T>` |
| `Services/ReservationService.cs` | Core business rules enforcement |
| `SQL/01_CreateDatabase.sql` | Raw SQL schema (optional – EF migrations preferred) |
| `SQL/02_SeedData.sql` | Sample data for testing |

## 🔐 Roles & Permissions

| Feature              | Admin | Staff |
|----------------------|:-----:|:-----:|
| View Dashboard       |  ✅   |  ✅   |
| Manage Vehicles      |  ✅   |  ✅   |
| Delete Vehicles      |  ✅   |  ❌   |
| Manage Customers     |  ✅   |  ✅   |
| Delete Customers     |  ✅   |  ❌   |
| Manage Reservations  |  ✅   |  ✅   |
| Manage Locations     |  ✅   |  ❌   |
| User Management      |  ✅   |  ❌   |

## 🧪 Run Unit Tests

```bash
cd Tests
dotnet test --logger "console;verbosity=normal"
```

## 📊 EF Core Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script --output schema.sql
```

## 🚀 Production Deployment

```bash
# Build release
dotnet publish -c Release -o ./publish

# The ./publish folder is ready for IIS deployment.
# Install .NET 8 Windows Hosting Bundle on the IIS server first.
```

---

## 📐 Business Rules Enforced

- ✅ Plate numbers are globally unique (even soft-deleted ones block reuse)
- ✅ NIC numbers are globally unique per customer
- ✅ Reservation start date cannot be in the past
- ✅ Reservation end date must be strictly after start date
- ✅ Vehicles can only be reserved when status = **Available**
- ✅ No overlapping reservations for the same vehicle
- ✅ Cannot delete a vehicle or customer with active/upcoming reservations
- ✅ Cancelling a reservation marks payment as **Refunded** and restores vehicle to **Available**
- ✅ Price is auto-calculated as `DailyRate × RentalDays`

---

*VehicleRent Pro – Enterprise-grade, built on SOLID principles & Clean Architecture.*
