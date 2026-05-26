-- ============================================================
--  Vehicle Rental Management System
--  Database: VehicleRentalDB
--  Author:   Senior Software Architect
--  Version:  1.0 | .NET 8 / SQL Server
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'VehicleRentalDB')
BEGIN
    ALTER DATABASE VehicleRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VehicleRentalDB;
END
GO

CREATE DATABASE VehicleRentalDB
    COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

USE VehicleRentalDB;
GO

-- ============================================================
--  ENUM-LIKE LOOKUP: PaymentStatus
-- ============================================================
CREATE TABLE PaymentStatus (
    StatusID   TINYINT      NOT NULL,
    StatusName NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_PaymentStatus PRIMARY KEY (StatusID),
    CONSTRAINT UQ_PaymentStatus_Name UNIQUE (StatusName)
);
INSERT INTO PaymentStatus VALUES (1, 'Pending'), (2, 'Paid'), (3, 'Refunded');
GO

-- ============================================================
--  ENUM-LIKE LOOKUP: AvailabilityStatus
-- ============================================================
CREATE TABLE AvailabilityStatus (
    StatusID   TINYINT      NOT NULL,
    StatusName NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_AvailabilityStatus PRIMARY KEY (StatusID),
    CONSTRAINT UQ_AvailabilityStatus_Name UNIQUE (StatusName)
);
INSERT INTO AvailabilityStatus VALUES (1, 'Available'), (2, 'Reserved'), (3, 'UnderMaintenance');
GO

-- ============================================================
--  TABLE: Location
-- ============================================================
CREATE TABLE Location (
    LocationID    INT           NOT NULL IDENTITY(1,1),
    Address       NVARCHAR(300) NOT NULL,
    ContactNumber NVARCHAR(20)  NOT NULL,
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Location          PRIMARY KEY (LocationID),
    CONSTRAINT CK_Location_Contact  CHECK (LEN(TRIM(ContactNumber)) >= 7)
);
GO

CREATE INDEX IX_Location_IsDeleted ON Location (IsDeleted);
GO

-- ============================================================
--  TABLE: Vehicle
-- ============================================================
CREATE TABLE Vehicle (
    PlateNumber        NVARCHAR(20)  NOT NULL,
    Manufacturer       NVARCHAR(100) NOT NULL,
    Model              NVARCHAR(100) NOT NULL,
    Year               SMALLINT      NOT NULL,
    AvailabilityStatus TINYINT       NOT NULL DEFAULT 1,
    LocationID         INT           NOT NULL,
    DailyRate          DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsDeleted          BIT           NOT NULL DEFAULT 0,
    CreatedAt          DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt          DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Vehicle              PRIMARY KEY (PlateNumber),
    CONSTRAINT UQ_Vehicle_Plate        UNIQUE (PlateNumber),
    CONSTRAINT FK_Vehicle_Location     FOREIGN KEY (LocationID) REFERENCES Location(LocationID),
    CONSTRAINT FK_Vehicle_AvailStatus  FOREIGN KEY (AvailabilityStatus) REFERENCES AvailabilityStatus(StatusID),
    CONSTRAINT CK_Vehicle_Year         CHECK (Year >= 1900 AND Year <= YEAR(GETDATE()) + 1),
    CONSTRAINT CK_Vehicle_DailyRate    CHECK (DailyRate >= 0)
);
GO

CREATE INDEX IX_Vehicle_LocationID         ON Vehicle (LocationID);
CREATE INDEX IX_Vehicle_AvailabilityStatus ON Vehicle (AvailabilityStatus);
CREATE INDEX IX_Vehicle_IsDeleted          ON Vehicle (IsDeleted);
CREATE INDEX IX_Vehicle_Manufacturer       ON Vehicle (Manufacturer);
GO

-- ============================================================
--  TABLE: Customer
-- ============================================================
CREATE TABLE Customer (
    NIC              NVARCHAR(20)  NOT NULL,
    Name             NVARCHAR(200) NOT NULL,
    Address          NVARCHAR(300) NOT NULL,
    MobilePhoneNumber NVARCHAR(20) NOT NULL,
    Email            NVARCHAR(200) NULL,
    IsDeleted        BIT           NOT NULL DEFAULT 0,
    CreatedAt        DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Customer           PRIMARY KEY (NIC),
    CONSTRAINT UQ_Customer_NIC       UNIQUE (NIC),
    CONSTRAINT CK_Customer_Mobile    CHECK (LEN(TRIM(MobilePhoneNumber)) >= 7),
    CONSTRAINT CK_Customer_Name      CHECK (LEN(TRIM(Name)) >= 2)
);
GO

CREATE INDEX IX_Customer_Name      ON Customer (Name);
CREATE INDEX IX_Customer_IsDeleted ON Customer (IsDeleted);
GO

-- ============================================================
--  TABLE: Reservation
-- ============================================================
CREATE TABLE Reservation (
    ReservationID   INT           NOT NULL IDENTITY(1,1),
    CustomerNIC     NVARCHAR(20)  NOT NULL,
    PlateNumber     NVARCHAR(20)  NOT NULL,
    RentalStartDate DATE          NOT NULL,
    RentalEndDate   DATE          NOT NULL,
    Price           DECIMAL(10,2) NOT NULL DEFAULT 0,
    PaymentStatus   TINYINT       NOT NULL DEFAULT 1,
    Notes           NVARCHAR(500) NULL,
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Reservation            PRIMARY KEY (ReservationID),
    CONSTRAINT FK_Reservation_Customer   FOREIGN KEY (CustomerNIC)   REFERENCES Customer(NIC),
    CONSTRAINT FK_Reservation_Vehicle    FOREIGN KEY (PlateNumber)   REFERENCES Vehicle(PlateNumber),
    CONSTRAINT FK_Reservation_Payment    FOREIGN KEY (PaymentStatus) REFERENCES PaymentStatus(StatusID),
    CONSTRAINT CK_Reservation_Dates      CHECK (RentalEndDate > RentalStartDate),
    CONSTRAINT CK_Reservation_Price      CHECK (Price >= 0)
);
GO

CREATE INDEX IX_Reservation_CustomerNIC   ON Reservation (CustomerNIC);
CREATE INDEX IX_Reservation_PlateNumber   ON Reservation (PlateNumber);
CREATE INDEX IX_Reservation_StartDate     ON Reservation (RentalStartDate);
CREATE INDEX IX_Reservation_EndDate       ON Reservation (RentalEndDate);
CREATE INDEX IX_Reservation_PaymentStatus ON Reservation (PaymentStatus);
CREATE INDEX IX_Reservation_IsDeleted     ON Reservation (IsDeleted);
GO

-- ============================================================
--  AspNetIdentity Tables are created by EF Core Migrations
-- ============================================================
PRINT 'VehicleRentalDB schema created successfully.';
GO
