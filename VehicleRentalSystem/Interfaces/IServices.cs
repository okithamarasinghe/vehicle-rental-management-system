using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Interfaces;

// ── Service Result Wrapper ────────────────────────────────────────────────────
public class ServiceResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IList<string> Errors { get; init; } = new List<string>();

    public static ServiceResult Ok(string message = "Operation successful.") =>
        new() { Success = true, Message = message };

    public static ServiceResult Fail(string error) =>
        new() { Success = false, Message = error, Errors = new List<string> { error } };

    public static ServiceResult Fail(IList<string> errors) =>
        new() { Success = false, Message = errors.FirstOrDefault() ?? "Validation failed.", Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string message = "Operation successful.") =>
        new() { Success = true, Message = message, Data = data };

    public new static ServiceResult<T> Fail(string error) =>
        new() { Success = false, Message = error, Errors = new List<string> { error } };
}

// ── Location Service ──────────────────────────────────────────────────────────
public interface ILocationService
{
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(LocationViewModel model);
    Task<ServiceResult> UpdateAsync(LocationViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<IEnumerable<Location>> SearchAsync(string searchTerm);
}

// ── Vehicle Service ───────────────────────────────────────────────────────────
public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByPlateAsync(string plateNumber);
    Task<ServiceResult> CreateAsync(VehicleViewModel model);
    Task<ServiceResult> UpdateAsync(VehicleViewModel model);
    Task<ServiceResult> DeleteAsync(string plateNumber);
    Task<IEnumerable<Vehicle>> SearchAsync(string? plate, string? manufacturer, string? model,
                                            VehicleAvailabilityStatus? status);
    Task<IEnumerable<Vehicle>> GetAvailableAsync();
    Task<ServiceResult> UpdateAvailabilityAsync(string plateNumber, VehicleAvailabilityStatus status);
}

// ── Customer Service ──────────────────────────────────────────────────────────
public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByNicAsync(string nic);
    Task<ServiceResult> CreateAsync(CustomerViewModel model);
    Task<ServiceResult> UpdateAsync(CustomerViewModel model);
    Task<ServiceResult> DeleteAsync(string nic);
    Task<IEnumerable<Customer>> SearchAsync(string? nic, string? name, string? mobile);
}

// ── Reservation Service ───────────────────────────────────────────────────────
public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(ReservationViewModel model);
    Task<ServiceResult> UpdateAsync(ReservationViewModel model);
    Task<ServiceResult> CancelAsync(int id);
    Task<ServiceResult> UpdatePaymentStatusAsync(int id, PaymentStatus status);
    Task<IEnumerable<Reservation>> SearchAsync(int? id, string? nic, string? plate,
                                                DateTime? start, DateTime? end);
    Task<IEnumerable<Reservation>> GetByCustomerAsync(string nic);
    Task<DashboardViewModel> GetDashboardDataAsync();
}
