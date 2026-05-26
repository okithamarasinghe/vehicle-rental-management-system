using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Reservation>> GetAllAsync()
        => await _dbSet.AsNoTracking()
                       .Include(r => r.Customer)
                       .Include(r => r.Vehicle).ThenInclude(v => v!.Location)
                       .OrderByDescending(r => r.CreatedAt)
                       .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetByCustomerAsync(string customerNic)
        => await _dbSet.AsNoTracking()
                       .Include(r => r.Vehicle).ThenInclude(v => v!.Location)
                       .Where(r => r.CustomerNIC == customerNic)
                       .OrderByDescending(r => r.RentalStartDate)
                       .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetByVehicleAsync(string plateNumber)
        => await _dbSet.AsNoTracking()
                       .Include(r => r.Customer)
                       .Where(r => r.PlateNumber == plateNumber)
                       .OrderByDescending(r => r.RentalStartDate)
                       .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
        => await _dbSet.AsNoTracking()
                       .Include(r => r.Customer)
                       .Include(r => r.Vehicle)
                       .Where(r => r.RentalEndDate >= DateTime.Today)
                       .OrderBy(r => r.RentalStartDate)
                       .ToListAsync();

    public async Task<IEnumerable<Reservation>> SearchAsync(int? reservationId, string? customerNic,
        string? plateNumber, DateTime? startDate, DateTime? endDate)
    {
        var q = _dbSet.AsNoTracking()
                      .Include(r => r.Customer)
                      .Include(r => r.Vehicle)
                      .AsQueryable();

        if (reservationId.HasValue)
            q = q.Where(r => r.ReservationID == reservationId.Value);
        if (!string.IsNullOrWhiteSpace(customerNic))
            q = q.Where(r => r.CustomerNIC.Contains(customerNic));
        if (!string.IsNullOrWhiteSpace(plateNumber))
            q = q.Where(r => r.PlateNumber.Contains(plateNumber));
        if (startDate.HasValue)
            q = q.Where(r => r.RentalStartDate >= startDate.Value);
        if (endDate.HasValue)
            q = q.Where(r => r.RentalEndDate <= endDate.Value);

        return await q.OrderByDescending(r => r.ReservationID).ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetWithDetailsAsync()
        => await _dbSet.AsNoTracking()
                       .Include(r => r.Customer)
                       .Include(r => r.Vehicle).ThenInclude(v => v!.Location)
                       .OrderByDescending(r => r.CreatedAt)
                       .ToListAsync();

    public async Task<Reservation?> GetWithDetailsAsync(int id)
        => await _dbSet.Include(r => r.Customer)
                       .Include(r => r.Vehicle).ThenInclude(v => v!.Location)
                       .FirstOrDefaultAsync(r => r.ReservationID == id);

    public async Task<decimal> GetTotalRevenueAsync()
        => await _dbSet.Where(r => r.PaymentStatus == PaymentStatus.Paid)
                       .SumAsync(r => r.Price);

    public async Task<IEnumerable<(int Year, int Month, decimal Revenue)>> GetMonthlyRevenueAsync(int months = 12)
    {
        var since = DateTime.Today.AddMonths(-months + 1);
        var data  = await _dbSet.AsNoTracking()
                                .Where(r => r.PaymentStatus == PaymentStatus.Paid &&
                                            r.RentalStartDate >= since)
                                .GroupBy(r => new { r.RentalStartDate.Year, r.RentalStartDate.Month })
                                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(r => r.Price) })
                                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                                .ToListAsync();
        return data.Select(d => (d.Year, d.Month, d.Revenue));
    }

    public async Task<IEnumerable<(int Year, int Month, int Count)>> GetMonthlyReservationCountAsync(int months = 12)
    {
        var since = DateTime.Today.AddMonths(-months + 1);
        var data  = await _dbSet.AsNoTracking()
                                .Where(r => r.RentalStartDate >= since)
                                .GroupBy(r => new { r.RentalStartDate.Year, r.RentalStartDate.Month })
                                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                                .ToListAsync();
        return data.Select(d => (d.Year, d.Month, d.Count));
    }
}
