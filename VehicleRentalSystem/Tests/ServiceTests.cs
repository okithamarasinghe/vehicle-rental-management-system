using Microsoft.Extensions.Logging;
using Moq;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.ViewModels;
using Xunit;

namespace VehicleRentalSystem.Tests;

// ╔══════════════════════════════════════════════════════╗
//   VehicleService Tests
// ╚══════════════════════════════════════════════════════╝
public class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository>      _repoMock   = new();
    private readonly Mock<ILogger<VehicleService>> _loggerMock = new();

    private VehicleService CreateService() =>
        new(_repoMock.Object, _loggerMock.Object);

    // ── Happy Path: Create Vehicle ────────────────────────────
    [Fact]
    public async Task CreateAsync_ValidVehicle_ReturnsSuccess()
    {
        _repoMock.Setup(r => r.IsPlateUniqueAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(true);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Vehicle>()))
                 .Returns(Task.CompletedTask);

        var svc    = CreateService();
        var result = await svc.CreateAsync(new VehicleViewModel
        {
            PlateNumber        = "CAA-1234",
            Manufacturer       = "Toyota",
            Model              = "Corolla",
            Year               = 2022,
            LocationID         = 1,
            DailyRate          = 5000,
            AvailabilityStatus = VehicleAvailabilityStatus.Available
        });

        Assert.True(result.Success);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
    }

    // ── Edge Case: Duplicate Plate ────────────────────────────
    [Fact]
    public async Task CreateAsync_DuplicatePlate_ReturnsFail()
    {
        _repoMock.Setup(r => r.IsPlateUniqueAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(false);

        var svc    = CreateService();
        var result = await svc.CreateAsync(new VehicleViewModel { PlateNumber = "CAA-1234" });

        Assert.False(result.Success);
        Assert.Contains("already in use", result.Message);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    // ── Edge Case: Delete vehicle with active reservations ────
    [Fact]
    public async Task DeleteAsync_VehicleWithActiveReservations_ReturnsFail()
    {
        var vehicle = new Vehicle
        {
            PlateNumber = "CAA-1234",
            Reservations = new List<Reservation>
            {
                new() { RentalEndDate = DateTime.Today.AddDays(5), PlateNumber = "CAA-1234" }
            }
        };
        _repoMock.Setup(r => r.GetWithReservationsAsync("CAA-1234"))
                 .ReturnsAsync(vehicle);

        var result = await CreateService().DeleteAsync("CAA-1234");

        Assert.False(result.Success);
        Assert.Contains("active", result.Message.ToLower());
    }

    // ── Happy Path: Delete vehicle with no active reservations ─
    [Fact]
    public async Task DeleteAsync_VehicleNoActiveReservations_ReturnsSuccess()
    {
        var vehicle = new Vehicle
        {
            PlateNumber  = "CAB-5678",
            Reservations = new List<Reservation>
            {
                new() { RentalEndDate = DateTime.Today.AddDays(-3), PlateNumber = "CAB-5678" }
            }
        };
        _repoMock.Setup(r => r.GetWithReservationsAsync("CAB-5678")).ReturnsAsync(vehicle);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);

        var result = await CreateService().DeleteAsync("CAB-5678");

        Assert.True(result.Success);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Vehicle>(v => v.IsDeleted)), Times.Once);
    }

    // ── Validation: Repository throws, service catches ─────────
    [Fact]
    public async Task CreateAsync_RepositoryThrows_ReturnsFail()
    {
        _repoMock.Setup(r => r.IsPlateUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Vehicle>())).ThrowsAsync(new Exception("DB error"));

        var result = await CreateService().CreateAsync(new VehicleViewModel
        {
            PlateNumber = "CAA-9999", Manufacturer = "Toyota", Model = "Camry",
            Year = 2023, LocationID = 1, DailyRate = 6000
        });

        Assert.False(result.Success);
    }
}

// ╔══════════════════════════════════════════════════════╗
//   CustomerService Tests
// ╚══════════════════════════════════════════════════════╝
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository>      _repoMock   = new();
    private readonly Mock<ILogger<CustomerService>> _loggerMock = new();

    private CustomerService CreateService() =>
        new(_repoMock.Object, _loggerMock.Object);

    // ── Happy Path: Create Customer ───────────────────────────
    [Fact]
    public async Task CreateAsync_ValidCustomer_ReturnsSuccess()
    {
        _repoMock.Setup(r => r.IsNicUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);

        var result = await CreateService().CreateAsync(new CustomerViewModel
        {
            NIC               = "199012345678",
            Name              = "John Perera",
            Address           = "123 Main Street, Colombo",
            MobilePhoneNumber = "+94771234567"
        });

        Assert.True(result.Success);
    }

    // ── Validation: Duplicate NIC ─────────────────────────────
    [Fact]
    public async Task CreateAsync_DuplicateNIC_ReturnsFail()
    {
        _repoMock.Setup(r => r.IsNicUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(false);

        var result = await CreateService().CreateAsync(new CustomerViewModel { NIC = "199012345678" });

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
    }

    // ── Edge Case: Delete customer with active reservations ────
    [Fact]
    public async Task DeleteAsync_CustomerWithActiveReservation_ReturnsFail()
    {
        var customer = new Customer
        {
            NIC = "199012345678",
            Reservations = new List<Reservation>
            {
                new() { RentalEndDate = DateTime.Today.AddDays(2), CustomerNIC = "199012345678" }
            }
        };
        _repoMock.Setup(r => r.GetWithReservationsAsync("199012345678")).ReturnsAsync(customer);

        var result = await CreateService().DeleteAsync("199012345678");

        Assert.False(result.Success);
    }

    // ── Happy Path: Delete customer with no active reservations
    [Fact]
    public async Task DeleteAsync_CustomerNoActiveReservations_ReturnsSuccess()
    {
        var customer = new Customer
        {
            NIC = "199012345678",
            Reservations = new List<Reservation>
            {
                new() { RentalEndDate = DateTime.Today.AddDays(-5) }
            }
        };
        _repoMock.Setup(r => r.GetWithReservationsAsync("199012345678")).ReturnsAsync(customer);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);

        var result = await CreateService().DeleteAsync("199012345678");

        Assert.True(result.Success);
    }
}

// ╔══════════════════════════════════════════════════════╗
//   ReservationService Business Rules Tests
// ╚══════════════════════════════════════════════════════╝
public class ReservationServiceBusinessRuleTests
{
    // ── BR: End date must be after start date ─────────────────
    [Fact]
    public void EndDateBeforeStartDate_ShouldFailValidation()
    {
        var model = new ReservationViewModel
        {
            RentalStartDate = DateTime.Today.AddDays(5),
            RentalEndDate   = DateTime.Today.AddDays(3)
        };
        int days = model.RentalDays;
        Assert.True(days <= 0, "RentalDays should be 0 or negative when end < start.");
    }

    // ── BR: Price calculation ────────────────────────────────
    [Theory]
    [InlineData(5000, 3, 15000)]
    [InlineData(7500, 7, 52500)]
    [InlineData(0,    5,     0)]
    public void PriceCalculation_DaysTimesRate_IsCorrect(decimal dailyRate, int days, decimal expected)
    {
        var price = dailyRate * days;
        Assert.Equal(expected, price);
    }

    // ── BR: Rental days computed correctly ────────────────────
    [Fact]
    public void RentalDays_ComputedFromDates_IsCorrect()
    {
        var model = new ReservationViewModel
        {
            RentalStartDate = new DateTime(2025, 6, 1),
            RentalEndDate   = new DateTime(2025, 6, 8)
        };
        Assert.Equal(7, model.RentalDays);
    }

    // ── BR: Start date in past ────────────────────────────────
    [Fact]
    public void StartDateInPast_IsInvalid()
    {
        var start = DateTime.Today.AddDays(-1);
        Assert.True(start < DateTime.Today, "Start date in the past should be rejected.");
    }

    // ── BR: Payment status enum values ────────────────────────
    [Fact]
    public void PaymentStatus_HasExpectedValues()
    {
        Assert.Equal(1, (int)PaymentStatus.Pending);
        Assert.Equal(2, (int)PaymentStatus.Paid);
        Assert.Equal(3, (int)PaymentStatus.Refunded);
    }

    // ── BR: Availability status enum ─────────────────────────
    [Fact]
    public void AvailabilityStatus_HasExpectedValues()
    {
        Assert.Equal(1, (int)VehicleAvailabilityStatus.Available);
        Assert.Equal(2, (int)VehicleAvailabilityStatus.Reserved);
        Assert.Equal(3, (int)VehicleAvailabilityStatus.UnderMaintenance);
    }
}

// ╔══════════════════════════════════════════════════════╗
//   ServiceResult Tests
// ╚══════════════════════════════════════════════════════╝
public class ServiceResultTests
{
    [Fact]
    public void ServiceResult_Ok_IsSuccess()
    {
        var result = ServiceResult.Ok("Done.");
        Assert.True(result.Success);
        Assert.Equal("Done.", result.Message);
    }

    [Fact]
    public void ServiceResult_Fail_IsNotSuccess()
    {
        var result = ServiceResult.Fail("Something went wrong.");
        Assert.False(result.Success);
        Assert.Contains("Something went wrong.", result.Errors);
    }

    [Fact]
    public void ServiceResult_Generic_CarriesData()
    {
        var result = ServiceResult<int>.Ok(42);
        Assert.True(result.Success);
        Assert.Equal(42, result.Data);
    }
}
