using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories;

public class LocationRepository : BaseRepository<Location>, ILocationRepository
{
    public LocationRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Location>> GetAllAsync()
        => await _dbSet.AsNoTracking()
                       .OrderBy(l => l.Address)
                       .ToListAsync();

    public async Task<IEnumerable<Location>> SearchAsync(string searchTerm)
    {
        var q = _dbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            q = q.Where(l => l.Address.ToLower().Contains(term) ||
                              l.ContactNumber.Contains(term));
        }
        return await q.OrderBy(l => l.Address).ToListAsync();
    }

    public async Task<Location?> GetWithVehiclesAsync(int id)
        => await _dbSet.Include(l => l.Vehicles)
                       .FirstOrDefaultAsync(l => l.LocationID == id);

    public async Task<int> GetVehicleCountAsync(int locationId)
        => await _context.Vehicles
                         .CountAsync(v => v.LocationID == locationId);
}
