-- ============================================================
--  Vehicle Rental Management System
--  Sample Seed Data – Run AFTER 01_CreateDatabase.sql
--  and AFTER EF Core Migrations have created Identity tables
-- ============================================================
USE VehicleRentalDB;
GO

-- ── Locations ─────────────────────────────────────────────────
INSERT INTO Location (Address, ContactNumber, IsDeleted, CreatedAt, UpdatedAt) VALUES
    ('No. 1, Galle Road, Colombo 03',          '+94112345678', 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('No. 45, Kandy Road, Kadawatha',           '+94112876543', 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('No. 12, Negombo Road, Wattala',           '+94113456789', 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('No. 78, High Level Road, Maharagama',     '+94114567890', 0, SYSUTCDATETIME(), SYSUTCDATETIME());
GO

-- ── Vehicles ──────────────────────────────────────────────────
-- AvailabilityStatus: 1=Available, 2=Reserved, 3=UnderMaintenance
INSERT INTO Vehicle (PlateNumber, Manufacturer, Model, Year, AvailabilityStatus, LocationID, DailyRate, IsDeleted, CreatedAt, UpdatedAt) VALUES
    ('CAA-1234', 'Toyota',  'Corolla',      2021, 1, 1, 6500.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAB-5678', 'Toyota',  'Aqua',         2020, 1, 1, 5500.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAC-9012', 'Honda',   'Fit',          2019, 1, 2, 5000.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAD-3456', 'Honda',   'Vezel',        2022, 2, 1, 8500.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAE-7890', 'Nissan',  'Leaf',         2023, 1, 2, 9000.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAF-2345', 'Suzuki',  'Alto',         2018, 1, 3, 3500.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAG-6789', 'Mazda',   'Axela',        2020, 3, 2, 7000.00,  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAH-0123', 'Mitsubishi','Outlander',  2021, 1, 4, 12000.00, 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAI-4567', 'BMW',     '3 Series',     2022, 1, 1, 18000.00, 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('CAJ-8901', 'Mercedes','C-Class',      2023, 3, 2, 20000.00, 0, SYSUTCDATETIME(), SYSUTCDATETIME());
GO

-- ── Customers ─────────────────────────────────────────────────
INSERT INTO Customer (NIC, Name, Address, MobilePhoneNumber, Email, IsDeleted, CreatedAt, UpdatedAt) VALUES
    ('199012345678',  'Kamal Perera',      'No. 10, Temple Road, Colombo 06',     '+94771234567', 'kamal@email.com',   0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('198507654321',  'Nilmini Fernando',  'No. 25, Station Road, Kandy',         '+94762345678', 'nilmini@email.com', 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('200001239876',  'Roshan Silva',      'No. 7, Beach Road, Negombo',          '+94753456789', 'roshan@email.com',  0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('197805432109',  'Priyanka Jayawardena', 'No. 55, Hospital Road, Gampaha',   '+94774567890', NULL,                0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('201012347890',  'Dilan Wickramasinghe', 'No. 3, School Lane, Kurunegala',   '+94765678901', 'dilan@email.com',   0, SYSUTCDATETIME(), SYSUTCDATETIME());
GO

-- ── Reservations ──────────────────────────────────────────────
-- PaymentStatus: 1=Pending, 2=Paid, 3=Refunded
DECLARE @tomorrow  DATE = DATEADD(DAY, 1,  CAST(GETDATE() AS DATE));
DECLARE @nextweek  DATE = DATEADD(DAY, 7,  CAST(GETDATE() AS DATE));
DECLARE @past1s    DATE = DATEADD(DAY, -14, CAST(GETDATE() AS DATE));
DECLARE @past1e    DATE = DATEADD(DAY, -10, CAST(GETDATE() AS DATE));
DECLARE @past2s    DATE = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
DECLARE @past2e    DATE = DATEADD(DAY, -25, CAST(GETDATE() AS DATE));

INSERT INTO Reservation (CustomerNIC, PlateNumber, RentalStartDate, RentalEndDate, Price, PaymentStatus, Notes, IsDeleted, CreatedAt, UpdatedAt) VALUES
    ('199012345678',  'CAD-3456', @tomorrow,  @nextweek,  59500.00, 1, 'Airport pickup needed',    0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('198507654321',  'CAJ-8901', @tomorrow,  DATEADD(DAY,4,@tomorrow), 80000.00, 2, NULL,           0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('200001239876',  'CAC-9012', @past1s,    @past1e,    20000.00, 2, 'Completed trip',           0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('197805432109',  'CAG-6789', @past2s,    @past2e,    35000.00, 2, 'Wedding car rental',       0, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('201012347890',  'CAC-9012', @past1s,    @past1e,    15000.00, 3, 'Cancelled – refunded',     1, SYSUTCDATETIME(), SYSUTCDATETIME());
GO

PRINT 'Seed data inserted successfully.';
GO
