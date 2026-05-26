using Microsoft.Extensions.Logging;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository     _repo;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository repo, ILogger<CustomerService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Customer?> GetByNicAsync(string nic)
        => await _repo.GetWithReservationsAsync(nic);

    public async Task<IEnumerable<Customer>> SearchAsync(string? nic, string? name, string? mobile)
        => await _repo.SearchAsync(nic, name, mobile);

    public async Task<ServiceResult> CreateAsync(CustomerViewModel model)
    {
        try
        {
            // BR: NIC must be unique
            if (!await _repo.IsNicUniqueAsync(model.NIC))
                return ServiceResult.Fail($"A customer with NIC '{model.NIC}' already exists.");

            var entity = new Customer
            {
                NIC              = model.NIC.Trim(),
                Name             = model.Name.Trim(),
                Address          = model.Address.Trim(),
                MobilePhoneNumber = model.MobilePhoneNumber.Trim(),
                Email            = model.Email?.Trim(),
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
            _logger.LogInformation("Customer created: NIC={NIC}", entity.NIC);
            return ServiceResult.Ok("Customer added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer.");
            return ServiceResult.Fail("An error occurred while adding the customer.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(CustomerViewModel model)
    {
        try
        {
            var entity = await _repo.GetByNicAsync(model.OriginalNIC ?? model.NIC);
            if (entity is null)
                return ServiceResult.Fail("Customer not found.");

            // If NIC changed, check uniqueness
            if (!string.Equals(entity.NIC, model.NIC, StringComparison.OrdinalIgnoreCase))
                if (!await _repo.IsNicUniqueAsync(model.NIC, entity.NIC))
                    return ServiceResult.Fail($"A customer with NIC '{model.NIC}' already exists.");

            entity.NIC               = model.NIC.Trim();
            entity.Name              = model.Name.Trim();
            entity.Address           = model.Address.Trim();
            entity.MobilePhoneNumber = model.MobilePhoneNumber.Trim();
            entity.Email             = model.Email?.Trim();
            entity.UpdatedAt         = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Customer updated: NIC={NIC}", entity.NIC);
            return ServiceResult.Ok("Customer updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer.");
            return ServiceResult.Fail("An error occurred while updating the customer.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(string nic)
    {
        try
        {
            var entity = await _repo.GetWithReservationsAsync(nic);
            if (entity is null)
                return ServiceResult.Fail("Customer not found.");

            if (entity.Reservations.Any(r => r.RentalEndDate >= DateTime.Today))
                return ServiceResult.Fail("Cannot delete a customer with active or upcoming reservations.");

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            _logger.LogInformation("Customer soft-deleted: NIC={NIC}", nic);
            return ServiceResult.Ok("Customer deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer NIC={NIC}", nic);
            return ServiceResult.Fail("An error occurred while deleting the customer.");
        }
    }
}
