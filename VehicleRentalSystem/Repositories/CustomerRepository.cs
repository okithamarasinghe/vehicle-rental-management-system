using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Customer>> GetAllAsync()
        => await _dbSet.AsNoTracking()
                       .OrderBy(c => c.Name)
                       .ToListAsync();

    public async Task<Customer?> GetByNicAsync(string nic)
        => await _dbSet.FirstOrDefaultAsync(c => c.NIC == nic);

    public async Task<IEnumerable<Customer>> SearchAsync(string? nic, string? name, string? mobile)
    {
        var q = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nic))
            q = q.Where(c => c.NIC.Contains(nic));
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(c => c.Name.ToLower().Contains(name.ToLower()));
        if (!string.IsNullOrWhiteSpace(mobile))
            q = q.Where(c => c.MobilePhoneNumber.Contains(mobile));

        return await q.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<bool> IsNicUniqueAsync(string nic, string? excludeNic = null)
        => !await _dbSet.IgnoreQueryFilters()
                        .AnyAsync(c => c.NIC == nic && c.NIC != excludeNic);

    public async Task<Customer?> GetWithReservationsAsync(string nic)
        => await _dbSet.Include(c => c.Reservations.Where(r => !r.IsDeleted))
                           .ThenInclude(r => r.Vehicle)
                       .FirstOrDefaultAsync(c => c.NIC == nic);
}
