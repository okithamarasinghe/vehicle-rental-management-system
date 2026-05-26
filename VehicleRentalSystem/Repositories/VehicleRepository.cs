using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories;

public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Vehicle>> GetAllAsync()
        => await _dbSet.AsNoTracking()
                       .Include(v => v.Location)
                       .OrderBy(v => v.PlateNumber)
                       .ToListAsync();

    public async Task<Vehicle?> GetByPlateAsync(string plateNumber)
        => await _dbSet.Include(v => v.Location)
                       .FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);

    public async Task<IEnumerable<Vehicle>> GetByLocationAsync(int locationId)
        => await _dbSet.AsNoTracking()
                       .Include(v => v.Location)
                       .Where(v => v.LocationID == locationId)
                       .OrderBy(v => v.PlateNumber)
                       .ToListAsync();

    public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        => await _dbSet.AsNoTracking()
                       .Include(v => v.Location)
                       .Where(v => v.AvailabilityStatus == VehicleAvailabilityStatus.Available)
                       .OrderBy(v => v.Manufacturer).ThenBy(v => v.Model)
                       .ToListAsync();

    public async Task<IEnumerable<Vehicle>> SearchAsync(string? plateNumber, string? manufacturer,
        string? model, VehicleAvailabilityStatus? status)
    {
        var q = _dbSet.AsNoTracking().Include(v => v.Location).AsQueryable();

        if (!string.IsNullOrWhiteSpace(plateNumber))
            q = q.Where(v => v.PlateNumber.Contains(plateNumber));
        if (!string.IsNullOrWhiteSpace(manufacturer))
            q = q.Where(v => v.Manufacturer.ToLower().Contains(manufacturer.ToLower()));
        if (!string.IsNullOrWhiteSpace(model))
            q = q.Where(v => v.Model.ToLower().Contains(model.ToLower()));
        if (status.HasValue)
            q = q.Where(v => v.AvailabilityStatus == status.Value);

        return await q.OrderBy(v => v.Manufacturer).ThenBy(v => v.Model).ToListAsync();
    }

    public async Task<bool> IsPlateUniqueAsync(string plateNumber, string? excludePlate = null)
    {
        // IgnoreQueryFilters so soft-deleted plates also block reuse
        return !await _dbSet.IgnoreQueryFilters()
                            .AnyAsync(v => v.PlateNumber == plateNumber &&
                                          v.PlateNumber != excludePlate);
    }

    public async Task<bool> HasOverlappingReservationAsync(string plateNumber, DateTime start,
        DateTime end, int? excludeReservationId = null)
        => await _context.Reservations
                         .AnyAsync(r => r.PlateNumber == plateNumber &&
                                        !r.IsDeleted &&
                                        r.ReservationID != excludeReservationId &&
                                        r.RentalStartDate < end &&
                                        r.RentalEndDate   > start);

    public async Task<Vehicle?> GetWithReservationsAsync(string plateNumber)
        => await _dbSet.Include(v => v.Location)
                       .Include(v => v.Reservations.Where(r => !r.IsDeleted))
                           .ThenInclude(r => r.Customer)
                       .FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);

    public async Task<IEnumerable<Vehicle>> GetVehiclesWithLocationAsync()
        => await _dbSet.AsNoTracking()
                       .Include(v => v.Location)
                       .OrderBy(v => v.Manufacturer)
                       .ToListAsync();
}
