using Microsoft.Extensions.Logging;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository      _repo;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(IVehicleRepository repo, ILogger<VehicleService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Vehicle?> GetByPlateAsync(string plateNumber)
        => await _repo.GetWithReservationsAsync(plateNumber);

    public async Task<IEnumerable<Vehicle>> GetAvailableAsync()
        => await _repo.GetAvailableVehiclesAsync();

    public async Task<IEnumerable<Vehicle>> SearchAsync(string? plate, string? manufacturer,
        string? model, VehicleAvailabilityStatus? status)
        => await _repo.SearchAsync(plate, manufacturer, model, status);

    public async Task<ServiceResult> CreateAsync(VehicleViewModel model)
    {
        try
        {
            if (!await _repo.IsPlateUniqueAsync(model.PlateNumber))
                return ServiceResult.Fail($"Plate number '{model.PlateNumber}' is already in use.");

            var entity = new Vehicle
            {
                PlateNumber        = model.PlateNumber.Trim().ToUpper(),
                Manufacturer       = model.Manufacturer.Trim(),
                Model              = model.Model.Trim(),
                Year               = model.Year,
                AvailabilityStatus = model.AvailabilityStatus,
                LocationID         = model.LocationID,
                DailyRate          = model.DailyRate,
                CreatedAt          = DateTime.UtcNow,
                UpdatedAt          = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
            _logger.LogInformation("Vehicle created: {PlateNumber}", entity.PlateNumber);
            return ServiceResult.Ok("Vehicle added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle.");
            return ServiceResult.Fail("An error occurred while adding the vehicle.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(VehicleViewModel model)
    {
        try
        {
            var entity = await _repo.GetByPlateAsync(model.OriginalPlateNumber ?? model.PlateNumber);
            if (entity is null)
                return ServiceResult.Fail("Vehicle not found.");

            if (!string.Equals(entity.PlateNumber, model.PlateNumber, StringComparison.OrdinalIgnoreCase))
                if (!await _repo.IsPlateUniqueAsync(model.PlateNumber, entity.PlateNumber))
                    return ServiceResult.Fail($"Plate number '{model.PlateNumber}' is already in use.");

            entity.PlateNumber        = model.PlateNumber.Trim().ToUpper();
            entity.Manufacturer       = model.Manufacturer.Trim();
            entity.Model              = model.Model.Trim();
            entity.Year               = model.Year;
            entity.AvailabilityStatus = model.AvailabilityStatus;
            entity.LocationID         = model.LocationID;
            entity.DailyRate          = model.DailyRate;
            entity.UpdatedAt          = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Vehicle updated: {PlateNumber}", entity.PlateNumber);
            return ServiceResult.Ok("Vehicle updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle.");
            return ServiceResult.Fail("An error occurred while updating the vehicle.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(string plateNumber)
    {
        try
        {
            var entity = await _repo.GetWithReservationsAsync(plateNumber);
            if (entity is null)
                return ServiceResult.Fail("Vehicle not found.");

            if (entity.Reservations.Any(r => r.RentalEndDate >= DateTime.Today))
                return ServiceResult.Fail("Cannot delete a vehicle that has active or upcoming reservations.");

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Vehicle soft-deleted: {PlateNumber}", plateNumber);
            return ServiceResult.Ok("Vehicle deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {Plate}", plateNumber);
            return ServiceResult.Fail("An error occurred while deleting the vehicle.");
        }
    }

    public async Task<ServiceResult> UpdateAvailabilityAsync(string plateNumber, VehicleAvailabilityStatus status)
    {
        try
        {
            var entity = await _repo.GetByPlateAsync(plateNumber);
            if (entity is null)
                return ServiceResult.Fail("Vehicle not found.");

            entity.AvailabilityStatus = status;
            entity.UpdatedAt          = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            return ServiceResult.Ok($"Vehicle status updated to {status}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for {Plate}", plateNumber);
            return ServiceResult.Fail("An error occurred.");
        }
    }
}
