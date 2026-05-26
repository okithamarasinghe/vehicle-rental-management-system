using Microsoft.Extensions.Logging;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository      _reservationRepo;
    private readonly IVehicleRepository          _vehicleRepo;
    private readonly ICustomerRepository         _customerRepo;
    private readonly ILocationRepository         _locationRepo;
    private readonly ApplicationDbContext        _context;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository      reservationRepo,
        IVehicleRepository          vehicleRepo,
        ICustomerRepository         customerRepo,
        ILocationRepository         locationRepo,
        ApplicationDbContext        context,
        ILogger<ReservationService> logger)
    {
        _reservationRepo = reservationRepo;
        _vehicleRepo     = vehicleRepo;
        _customerRepo    = customerRepo;
        _locationRepo    = locationRepo;
        _context         = context;
        _logger          = logger;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
        => await _reservationRepo.GetWithDetailsAsync();

    public async Task<Reservation?> GetByIdAsync(int id)
        => await _reservationRepo.GetWithDetailsAsync(id);

    public async Task<IEnumerable<Reservation>> GetByCustomerAsync(string nic)
        => await _reservationRepo.GetByCustomerAsync(nic);

    public async Task<IEnumerable<Reservation>> SearchAsync(int? id, string? nic, string? plate,
        DateTime? start, DateTime? end)
        => await _reservationRepo.SearchAsync(id, nic, plate, start, end);

    public async Task<ServiceResult> CreateAsync(ReservationViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // BR: Dates must not be in the past
            if (model.RentalStartDate.Date < DateTime.Today)
                errors.Add("Rental start date cannot be in the past.");

            // BR: End date must be after start date
            if (model.RentalEndDate <= model.RentalStartDate)
                errors.Add("Rental end date must be after the start date.");

            if (errors.Any())
                return ServiceResult.Fail(errors);

            // BR: Customer must exist
            var customer = await _customerRepo.GetByNicAsync(model.CustomerNIC);
            if (customer is null)
                return ServiceResult.Fail("Customer not found.");

            // BR: Vehicle must exist and be available
            var vehicle = await _vehicleRepo.GetByPlateAsync(model.PlateNumber);
            if (vehicle is null)
                return ServiceResult.Fail("Vehicle not found.");

            if (vehicle.AvailabilityStatus != VehicleAvailabilityStatus.Available)
                return ServiceResult.Fail($"Vehicle '{model.PlateNumber}' is not available for rental.");

            // BR: No overlapping reservations
            var overlaps = await _vehicleRepo.HasOverlappingReservationAsync(
                model.PlateNumber, model.RentalStartDate, model.RentalEndDate);
            if (overlaps)
                return ServiceResult.Fail("The selected vehicle already has a reservation overlapping those dates.");

            // Auto-calculate price if not provided
            if (model.Price == 0 && vehicle.DailyRate > 0)
            {
                int days    = (model.RentalEndDate - model.RentalStartDate).Days;
                model.Price = vehicle.DailyRate * days;
            }

            var entity = new Reservation
            {
                CustomerNIC     = model.CustomerNIC,
                PlateNumber     = model.PlateNumber,
                RentalStartDate = model.RentalStartDate.Date,
                RentalEndDate   = model.RentalEndDate.Date,
                Price           = model.Price,
                PaymentStatus   = model.PaymentStatus,
                Notes           = model.Notes?.Trim(),
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };

            await _reservationRepo.AddAsync(entity);

            // Update vehicle status to Reserved
            vehicle.AvailabilityStatus = VehicleAvailabilityStatus.Reserved;
            vehicle.UpdatedAt          = DateTime.UtcNow;
            await _vehicleRepo.UpdateAsync(vehicle);

            _logger.LogInformation("Reservation created: ID={ID} for Vehicle={Plate}",
                entity.ReservationID, entity.PlateNumber);
            return ServiceResult.Ok($"Reservation #{entity.ReservationID} created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation.");
            return ServiceResult.Fail("An error occurred while creating the reservation.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(ReservationViewModel model)
    {
        try
        {
            var entity = await _reservationRepo.GetWithDetailsAsync(model.ReservationID);
            if (entity is null)
                return ServiceResult.Fail("Reservation not found.");

            var errors = new List<string>();
            if (model.RentalEndDate <= model.RentalStartDate)
                errors.Add("Rental end date must be after the start date.");
            if (errors.Any())
                return ServiceResult.Fail(errors);

            // Check overlaps excluding self
            var overlaps = await _vehicleRepo.HasOverlappingReservationAsync(
                model.PlateNumber, model.RentalStartDate, model.RentalEndDate, model.ReservationID);
            if (overlaps)
                return ServiceResult.Fail("The selected vehicle has overlapping reservations for those dates.");

            entity.CustomerNIC     = model.CustomerNIC;
            entity.PlateNumber     = model.PlateNumber;
            entity.RentalStartDate = model.RentalStartDate.Date;
            entity.RentalEndDate   = model.RentalEndDate.Date;
            entity.Price           = model.Price;
            entity.PaymentStatus   = model.PaymentStatus;
            entity.Notes           = model.Notes?.Trim();
            entity.UpdatedAt       = DateTime.UtcNow;

            await _reservationRepo.UpdateAsync(entity);
            _logger.LogInformation("Reservation updated: ID={ID}", entity.ReservationID);
            return ServiceResult.Ok("Reservation updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation.");
            return ServiceResult.Fail("An error occurred while updating the reservation.");
        }
    }

    public async Task<ServiceResult> CancelAsync(int id)
    {
        try
        {
            var entity = await _reservationRepo.GetWithDetailsAsync(id);
            if (entity is null)
                return ServiceResult.Fail("Reservation not found.");

            entity.IsDeleted     = true;
            entity.PaymentStatus = PaymentStatus.Refunded;
            entity.UpdatedAt     = DateTime.UtcNow;
            await _reservationRepo.UpdateAsync(entity);

            // Set vehicle back to available if no other active reservations exist
            var otherActive = await _vehicleRepo.HasOverlappingReservationAsync(
                entity.PlateNumber, DateTime.Today, DateTime.MaxValue, id);
            if (!otherActive && entity.Vehicle is not null)
            {
                entity.Vehicle.AvailabilityStatus = VehicleAvailabilityStatus.Available;
                entity.Vehicle.UpdatedAt           = DateTime.UtcNow;
                await _vehicleRepo.UpdateAsync(entity.Vehicle);
            }

            _logger.LogInformation("Reservation cancelled: ID={ID}", id);
            return ServiceResult.Ok("Reservation cancelled successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation ID={ID}", id);
            return ServiceResult.Fail("An error occurred while cancelling the reservation.");
        }
    }

    public async Task<ServiceResult> UpdatePaymentStatusAsync(int id, PaymentStatus status)
    {
        try
        {
            var entity = await _reservationRepo.GetByIdAsync(id);
            if (entity is null)
                return ServiceResult.Fail("Reservation not found.");

            entity.PaymentStatus = status;
            entity.UpdatedAt     = DateTime.UtcNow;
            await _reservationRepo.UpdateAsync(entity);
            return ServiceResult.Ok($"Payment status updated to {status}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status.");
            return ServiceResult.Fail("An error occurred.");
        }
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var vehicles     = (await _vehicleRepo.GetAllAsync()).ToList();
        var customers    = (await _customerRepo.GetAllAsync()).ToList();
        var locations    = (await _locationRepo.GetAllAsync()).ToList();
        var reservations = (await _reservationRepo.GetWithDetailsAsync()).ToList();

        var monthlyRevenue = (await _reservationRepo.GetMonthlyRevenueAsync(12)).ToList();
        var monthlyCount   = (await _reservationRepo.GetMonthlyReservationCountAsync(12)).ToList();

        var locationBreakdown = locations.Select(l => new LocationVehicleCount
        {
            Location = l.Address.Length > 30 ? l.Address[..30] + "…" : l.Address,
            Count    = vehicles.Count(v => v.LocationID == l.LocationID)
        }).ToList();

        var recent = reservations.Take(10).Select(r => new ReservationListItemViewModel
        {
            ReservationID  = r.ReservationID,
            CustomerName   = r.Customer?.Name ?? r.CustomerNIC,
            CustomerNIC    = r.CustomerNIC,
            PlateNumber    = r.PlateNumber,
            VehicleDetails = r.Vehicle is null ? r.PlateNumber : $"{r.Vehicle.Manufacturer} {r.Vehicle.Model}",
            StartDate      = r.RentalStartDate,
            EndDate        = r.RentalEndDate,
            Price          = r.Price,
            Payment        = r.PaymentStatus,
            IsActive       = r.IsActive
        }).ToList();

        var today = DateTime.Today;
        decimal monthRevenue = reservations
            .Where(r => r.PaymentStatus == PaymentStatus.Paid &&
                        r.RentalStartDate.Year  == today.Year  &&
                        r.RentalStartDate.Month == today.Month)
            .Sum(r => r.Price);

        return new DashboardViewModel
        {
            TotalVehicles       = vehicles.Count,
            AvailableVehicles   = vehicles.Count(v => v.AvailabilityStatus == VehicleAvailabilityStatus.Available),
            ReservedVehicles    = vehicles.Count(v => v.AvailabilityStatus == VehicleAvailabilityStatus.Reserved),
            MaintenanceVehicles = vehicles.Count(v => v.AvailabilityStatus == VehicleAvailabilityStatus.UnderMaintenance),
            TotalCustomers      = customers.Count,
            TotalLocations      = locations.Count,
            ActiveReservations  = reservations.Count(r => r.IsActive),
            TotalReservations   = reservations.Count,
            TotalRevenue        = await _reservationRepo.GetTotalRevenueAsync(),
            MonthRevenue        = monthRevenue,
            RecentReservations  = recent,
            MonthlyRevenue      = monthlyRevenue.Select(m => new MonthlyChartPoint
            {
                Label = $"{m.Year}-{m.Month:D2}",
                Value = m.Revenue
            }).ToList(),
            MonthlyCount = monthlyCount.Select(m => new MonthlyChartPoint
            {
                Label = $"{m.Year}-{m.Month:D2}",
                Value = m.Count
            }).ToList(),
            LocationBreakdown = locationBreakdown
        };
    }
}
