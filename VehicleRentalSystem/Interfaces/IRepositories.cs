using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Interfaces;

// ── Generic Repository ────────────────────────────────────────────────────────
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<bool> ExistsAsync(object id);
}

// ── Location Repository ───────────────────────────────────────────────────────
public interface ILocationRepository : IRepository<Location>
{
    Task<IEnumerable<Location>> SearchAsync(string searchTerm);
    Task<Location?> GetWithVehiclesAsync(int id);
    Task<int> GetVehicleCountAsync(int locationId);
}

// ── Vehicle Repository ────────────────────────────────────────────────────────
public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByPlateAsync(string plateNumber);
    Task<IEnumerable<Vehicle>> GetByLocationAsync(int locationId);
    Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
    Task<IEnumerable<Vehicle>> SearchAsync(string? plateNumber, string? manufacturer, string? model,
                                            VehicleAvailabilityStatus? status);
    Task<bool> IsPlateUniqueAsync(string plateNumber, string? excludePlate = null);
    Task<bool> HasOverlappingReservationAsync(string plateNumber, DateTime start, DateTime end,
                                               int? excludeReservationId = null);
    Task<Vehicle?> GetWithReservationsAsync(string plateNumber);
    Task<IEnumerable<Vehicle>> GetVehiclesWithLocationAsync();
}

// ── Customer Repository ───────────────────────────────────────────────────────
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByNicAsync(string nic);
    Task<IEnumerable<Customer>> SearchAsync(string? nic, string? name, string? mobile);
    Task<bool> IsNicUniqueAsync(string nic, string? excludeNic = null);
    Task<Customer?> GetWithReservationsAsync(string nic);
}

// ── Reservation Repository ────────────────────────────────────────────────────
public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByCustomerAsync(string customerNic);
    Task<IEnumerable<Reservation>> GetByVehicleAsync(string plateNumber);
    Task<IEnumerable<Reservation>> GetActiveReservationsAsync();
    Task<IEnumerable<Reservation>> SearchAsync(int? reservationId, string? customerNic,
                                                string? plateNumber, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<Reservation>> GetWithDetailsAsync();
    Task<Reservation?> GetWithDetailsAsync(int id);
    Task<decimal> GetTotalRevenueAsync();
    Task<IEnumerable<(int Year, int Month, decimal Revenue)>> GetMonthlyRevenueAsync(int months = 12);
    Task<IEnumerable<(int Year, int Month, int Count)>> GetMonthlyReservationCountAsync(int months = 12);
}
