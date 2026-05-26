using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Data;

/// <summary>
/// EF Core DbContext. Extends IdentityDbContext for ASP.NET Identity.
/// Soft-delete via global query filters on IsDeleted.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── DbSets ──────────────────────────────────────────────────────────────
    public DbSet<Location>    Locations    => Set<Location>();
    public DbSet<Vehicle>     Vehicles     => Set<Vehicle>();
    public DbSet<Customer>    Customers    => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // ── Fluent API ───────────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Global Soft-Delete Filters ───────────────────────────────────────
        builder.Entity<Location>   ().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Vehicle>    ().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Customer>   ().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted);

        // ── Location ─────────────────────────────────────────────────────────
        builder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");
            entity.HasKey(e => e.LocationID);
            entity.Property(e => e.Address)
                  .IsRequired()
                  .HasMaxLength(300);
            entity.Property(e => e.ContactNumber)
                  .IsRequired()
                  .HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.IsDeleted);
        });

        // ── Vehicle ───────────────────────────────────────────────────────────
        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicle");
            entity.HasKey(e => e.PlateNumber);
            entity.Property(e => e.PlateNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.PlateNumber).IsUnique();
            entity.Property(e => e.Manufacturer).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Model).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DailyRate)
                  .HasColumnType("decimal(10,2)")
                  .HasDefaultValue(0m);
            entity.Property(e => e.AvailabilityStatus)
                  .HasConversion<byte>()
                  .HasDefaultValue(VehicleAvailabilityStatus.Available);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(v => v.Location)
                  .WithMany(l => l.Vehicles)
                  .HasForeignKey(v => v.LocationID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.LocationID);
            entity.HasIndex(e => e.AvailabilityStatus);
            entity.HasIndex(e => e.IsDeleted);
        });

        // ── Customer ──────────────────────────────────────────────────────────
        builder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");
            entity.HasKey(e => e.NIC);
            entity.Property(e => e.NIC).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.NIC).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(300).IsRequired();
            entity.Property(e => e.MobilePhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsDeleted);
        });

        // ── Reservation ───────────────────────────────────────────────────────
        builder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservation");
            entity.HasKey(e => e.ReservationID);
            entity.Property(e => e.ReservationID).ValueGeneratedOnAdd();
            entity.Property(e => e.CustomerNIC).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PlateNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Price)
                  .HasColumnType("decimal(10,2)")
                  .HasDefaultValue(0m);
            entity.Property(e => e.PaymentStatus)
                  .HasConversion<byte>()
                  .HasDefaultValue(PaymentStatus.Pending);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(r => r.Customer)
                  .WithMany(c => c.Reservations)
                  .HasForeignKey(r => r.CustomerNIC)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Vehicle)
                  .WithMany(v => v.Reservations)
                  .HasForeignKey(r => r.PlateNumber)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CustomerNIC);
            entity.HasIndex(e => e.PlateNumber);
            entity.HasIndex(e => e.RentalStartDate);
            entity.HasIndex(e => e.RentalEndDate);
            entity.HasIndex(e => e.PaymentStatus);
            entity.HasIndex(e => e.IsDeleted);
        });
    }
}
