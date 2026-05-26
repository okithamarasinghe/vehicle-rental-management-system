using Microsoft.Extensions.Logging;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repo;
    private readonly ILogger<LocationService> _logger;

    public LocationService(ILocationRepository repo, ILogger<LocationService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<Location>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Location?> GetByIdAsync(int id)
        => await _repo.GetByIdAsync(id);

    public async Task<IEnumerable<Location>> SearchAsync(string searchTerm)
        => await _repo.SearchAsync(searchTerm);

    public async Task<ServiceResult> CreateAsync(LocationViewModel model)
    {
        try
        {
            var entity = new Location
            {
                Address       = model.Address.Trim(),
                ContactNumber = model.ContactNumber.Trim(),
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
            _logger.LogInformation("Location created: {Address}", entity.Address);
            return ServiceResult.Ok("Location created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location.");
            return ServiceResult.Fail("An error occurred while creating the location.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(LocationViewModel model)
    {
        try
        {
            var entity = await _repo.GetByIdAsync(model.LocationID);
            if (entity is null)
                return ServiceResult.Fail("Location not found.");

            entity.Address       = model.Address.Trim();
            entity.ContactNumber = model.ContactNumber.Trim();
            entity.UpdatedAt     = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Location updated: ID={ID}", model.LocationID);
            return ServiceResult.Ok("Location updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location ID={ID}", model.LocationID);
            return ServiceResult.Fail("An error occurred while updating the location.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var count = await _repo.GetVehicleCountAsync(id);
            if (count > 0)
                return ServiceResult.Fail("Cannot delete a location that has vehicles assigned to it.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity is null)
                return ServiceResult.Fail("Location not found.");

            // Soft delete
            entity.IsDeleted  = true;
            entity.UpdatedAt  = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Location soft-deleted: ID={ID}", id);
            return ServiceResult.Ok("Location deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location ID={ID}", id);
            return ServiceResult.Fail("An error occurred while deleting the location.");
        }
    }
}
