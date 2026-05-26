# Vehicle Rental Management System (VRMS)
### Complete Technical Documentation & Deployment Guide
**Technology Stack:** C# · .NET 8 · ASP.NET Core MVC · Entity Framework Core · SQL Server · Bootstrap 5

---

## PART 1 – ER DIAGRAM ANALYSIS

### 1.1 Entities Identified

| Entity       | Primary Key    | Type          | Notes                         |
|--------------|----------------|---------------|-------------------------------|
| LOCATION     | LocationID     | INT (IDENTITY)| Physical rental branch        |
| VEHICLE      | PlateNumber    | NVARCHAR(20)  | Natural PK, unique plate      |
| CUSTOMER     | NIC            | NVARCHAR(20)  | Natural PK, Sri Lanka NIC     |
| RESERVATION  | ReservationID  | INT (IDENTITY)| Transactional entity          |

### 1.2 Attributes per Entity

| Entity      | Attributes                                                              |
|-------------|-------------------------------------------------------------------------|
| Location    | LocationID (PK), Address, ContactNumber                                |
| Vehicle     | PlateNumber (PK), Manufacturer, Model, Year, AvailabilityStatus, LocationID (FK) |
| Customer    | NIC (PK), Name, Address, MobilePhoneNumber                             |
| Reservation | ReservationID (PK), CustomerID (FK→NIC), VehicleID (FK→PlateNumber), RentalStartDate, RentalEndDate, Price, PaymentStatus |

### 1.3 Relationships & Cardinalities

| Relationship         | Type  | Description                                              |
|----------------------|-------|----------------------------------------------------------|
| LOCATION "located at" VEHICLE | 1 : N | One location can have many vehicles. Each vehicle belongs to exactly one location. |
| VEHICLE "reserved in" RESERVATION | 1 : N | One vehicle can appear in many reservations (over time). Each reservation is for exactly one vehicle. |
| CUSTOMER "makes" RESERVATION | 1 : N | One customer can make many reservations. Each reservation belongs to exactly one customer. |

### 1.4 Business Rules from ER Diagram

1. A vehicle must always belong to a location (mandatory participation).
2. A reservation must always reference a valid customer and a valid vehicle.
3. Vehicle availability status governs whether it can be reserved.
4. Payment status tracks financial state of each reservation.
5. Plate number is a natural unique key for vehicles.
6. NIC is a natural unique key for customers.

### 1.5 Suggested Improvements (Applied in Implementation)

| Gap                | Improvement Applied                                          |
|--------------------|--------------------------------------------------------------|
| No daily rate      | Added `DailyRate` to Vehicle for price calculation           |
| No email on customer | Added optional `Email` field                              |
| No notes on reservation | Added `Notes` field                                  |
| No soft delete     | Added `IsDeleted` flag on all entities (safe deletion)       |
| No audit trail     | Added `CreatedAt` / `UpdatedAt` timestamps                   |
| Missing constraints | Added CHECK constraints, UNIQUE, INDEXES                    |

---

## PART 2 – SYSTEM ARCHITECTURE

### 2.1 N-Tier Architecture Layers

```
┌─────────────────────────────────────────┐
│       PRESENTATION LAYER                │
│  Controllers · Razor Views · Bootstrap  │
│  ViewModels · Tag Helpers               │
├─────────────────────────────────────────┤
│       BUSINESS LAYER (Services)         │
│  LocationService · VehicleService       │
│  CustomerService · ReservationService   │
│  Business Rules Enforcement             │
├─────────────────────────────────────────┤
│       DATA ACCESS LAYER (Repositories)  │
│  EF Core · LINQ · Async Queries         │
│  Repository Pattern · Unit of Work      │
├─────────────────────────────────────────┤
│       DATABASE LAYER                    │
│  SQL Server · Tables · Indexes · FKs    │
│  Constraints · Stored Logic             │
└─────────────────────────────────────────┘
```

### 2.2 Complete Project Folder Structure

```
VehicleRentalSystem/
│
├── Controllers/
│   ├── HomeController.cs         # Dashboard
│   ├── VehicleController.cs      # Vehicle CRUD + search
│   ├── CustomerController.cs     # Customer CRUD + search
│   ├── ReservationController.cs  # Reservation CRUD + cancel
│   ├── LocationController.cs     # Location CRUD
│   └── AccountController.cs      # Login / Register / UserList
│
├── Models/
│   ├── Location.cs               # Location entity
│   ├── Vehicle.cs                # Vehicle entity + enum
│   ├── Customer.cs               # Customer entity
│   └── Reservation.cs            # Reservation entity + enum
│
├── ViewModels/
│   └── ViewModels.cs             # All ViewModels in one file
│
├── Interfaces/
│   ├── IRepositories.cs          # All repository interfaces
│   └── IServices.cs              # All service interfaces + ServiceResult
│
├── Repositories/
│   ├── BaseRepository.cs         # Generic base repository
│   ├── LocationRepository.cs
│   ├── VehicleRepository.cs
│   ├── CustomerRepository.cs
│   └── ReservationRepository.cs
│
├── Services/
│   ├── LocationService.cs
│   ├── VehicleService.cs
│   ├── CustomerService.cs
│   └── ReservationService.cs
│
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   ├── ApplicationUser.cs        # Extended Identity User
│   └── Migrations/               # EF Core migrations (auto-generated)
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml        # Master layout + sidebar
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── Home/
│   │   └── Index.cshtml          # Dashboard with Chart.js
│   ├── Vehicle/
│   │   ├── Index.cshtml          # Vehicle list + search
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   ├── Customer/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   ├── Reservation/
│   │   ├── Index.cshtml          # List + search/filter
│   │   ├── Create.cshtml         # Auto price calculation
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Cancel.cshtml
│   ├── Location/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── UserList.cshtml
│   │   └── AccessDenied.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── wwwroot/
│   ├── css/site.css              # Custom styles
│   └── js/site.js                # Sidebar toggle + utilities
│
├── SQL/
│   └── 01_CreateDatabase.sql     # Full schema script
│
├── Tests/
│   ├── ServiceTests.cs           # xUnit tests
│   └── VehicleRentalSystem.Tests.csproj
│
├── Program.cs                    # App startup + DI + seeding
├── appsettings.json
├── appsettings.Development.json
└── VehicleRentalSystem.csproj
```

---

## PART 3 – BUSINESS RULES ENFORCED

### Vehicle Rules
| Rule | Where Enforced |
|------|----------------|
| Plate number must be unique | `VehicleRepository.IsPlateUniqueAsync()` + DB UNIQUE constraint |
| Vehicle cannot be reserved if unavailable | `ReservationService.CreateAsync()` – checks AvailabilityStatus |
| Vehicle cannot have overlapping reservations | `VehicleRepository.HasOverlappingReservationAsync()` – date range query |
| Cannot delete vehicle with active reservations | `VehicleService.DeleteAsync()` – checks future end dates |

### Reservation Rules
| Rule | Where Enforced |
|------|----------------|
| End date must be after start date | `ReservationService.CreateAsync()` + `UpdateAsync()` |
| Cannot make reservation in the past | `ReservationService.CreateAsync()` – start date ≥ today |
| Customer must exist | `ReservationService` – fetches customer, returns error if null |
| Vehicle must exist | `ReservationService` – fetches vehicle, returns error if null |
| Auto price calculation | `ReservationService` – DailyRate × Days |

### Customer Rules
| Rule | Where Enforced |
|------|----------------|
| NIC must be unique | `CustomerRepository.IsNicUniqueAsync()` + DB UNIQUE constraint |
| Mobile number format validation | Data Annotations regex + client-side validation |

### Payment Status
| Value | Meaning |
|-------|---------|
| 1 – Pending | Default when reservation created |
| 2 – Paid | Payment confirmed |
| 3 – Refunded | After reservation cancellation |

---

## PART 4 – AUTHENTICATION & AUTHORIZATION

### Roles
| Role  | Permissions |
|-------|-------------|
| Admin | Full CRUD on all entities, user management, delete operations |
| Staff | Create/Edit customers and reservations; read vehicles/locations; cannot delete |

### Default Admin Credentials (seeded automatically)
```
Email:    admin@vrms.lk
Password: Admin@12345
```
> **Important:** Change the default password after first login in production.

---

## PART 5 – DEPLOYMENT GUIDE

### 5.1 Prerequisites

- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- SQL Server 2019+ or LocalDB (development)
- Visual Studio 2022 or VS Code
- IIS (production) or Kestrel

### 5.2 Local Development Setup

```bash
# 1. Clone the repository
git clone https://github.com/yourrepo/VehicleRentalSystem.git
cd VehicleRentalSystem

# 2. Restore NuGet packages
dotnet restore

# 3. Update connection string in appsettings.json
# (LocalDB is pre-configured for development)

# 4. Apply EF Core migrations (auto-runs on startup too)
dotnet ef migrations add InitialCreate --project VehicleRentalSystem.csproj
dotnet ef database update

# 5. Run the application
dotnet run

# Application starts at https://localhost:5001
# Default login: admin@vrms.lk / Admin@12345
```

### 5.3 EF Core Migration Commands

```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Revert last migration
dotnet ef migrations remove

# Generate SQL script from migrations
dotnet ef migrations script --output migration.sql

# Drop and recreate (development only!)
dotnet ef database drop --force
dotnet ef database update
```

### 5.4 Production SQL Server Setup

```sql
-- Update appsettings.json for production:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=VehicleRentalDB;User Id=vrms_user;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

### 5.5 Publish to IIS

```bash
# Step 1: Publish release build
dotnet publish -c Release -o ./publish

# Step 2: In IIS Manager
# - Create new website pointing to ./publish folder
# - Application Pool: No Managed Code
# - Install the .NET 8 Hosting Bundle on the server:
#   https://dotnet.microsoft.com/download/dotnet/8.0 → Hosting Bundle

# Step 3: Set ASPNETCORE_ENVIRONMENT to Production in IIS
```

### 5.6 Production web.config (IIS)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\VehicleRentalSystem.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## PART 6 – PRODUCTION CHECKLIST

| # | Item | Status |
|---|------|--------|
| 1 | Change default admin password | ⬜ |
| 2 | Use production SQL Server (not LocalDB) | ⬜ |
| 3 | Set ASPNETCORE_ENVIRONMENT = Production | ⬜ |
| 4 | Enable HTTPS with valid SSL certificate | ⬜ |
| 5 | Set up Serilog file sink with log rotation | ⬜ |
| 6 | Configure SQL Server backups | ⬜ |
| 7 | Install .NET 8 Hosting Bundle on server | ⬜ |
| 8 | Apply DB migrations on production | ⬜ |
| 9 | Test all user roles and permissions | ⬜ |
| 10 | Configure Windows Firewall for SQL Server | ⬜ |
| 11 | Set up Application Pool with proper identity | ⬜ |
| 12 | Enable Windows Event Log for errors | ⬜ |

---

## PART 7 – DESIGN PATTERNS & PRINCIPLES

| Principle | Applied Where |
|-----------|---------------|
| **Repository Pattern** | `BaseRepository<T>` + 4 entity repositories |
| **Dependency Injection** | All services/repositories registered in `Program.cs` |
| **SOLID – SRP** | Each class has one responsibility (service, repo, controller) |
| **SOLID – OCP** | `IRepository<T>` generic interface; extend without modifying |
| **SOLID – LSP** | Concrete repos substitute their interface without issues |
| **SOLID – ISP** | Separate interfaces per entity (`IVehicleRepository`, etc.) |
| **SOLID – DIP** | Controllers depend on service interfaces, not implementations |
| **Soft Delete** | `IsDeleted` flag + EF global query filters |
| **ViewModel Pattern** | Decoupled presentation from domain models |
| **Service Layer** | All business logic in `*Service.cs`, not in controllers |
| **N-Tier Architecture** | Presentation → Business → Data Access → Database |

---

## PART 8 – RUNNING UNIT TESTS

```bash
# Run all tests
dotnet test Tests/VehicleRentalSystem.Tests.csproj

# Run with verbose output
dotnet test Tests/VehicleRentalSystem.Tests.csproj --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~VehicleServiceTests"
```

### Test Coverage Summary

| Test Class | Tests | Covers |
|------------|-------|--------|
| `VehicleServiceTests` | 4 | Create, Duplicate Plate, Delete with/without reservations, Exception |
| `CustomerServiceTests` | 4 | Create, Duplicate NIC, Delete with/without reservations |
| `ReservationServiceBusinessRuleTests` | 6 | Date validation, price calculation, enum values |
| `ServiceResultTests` | 3 | Result wrapper behavior |

---

*Generated by VehicleRent Pro Generator | Enterprise-grade .NET 8 Architecture*
