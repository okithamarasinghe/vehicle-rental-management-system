using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Middleware;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;
using VehicleRentalSystem.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/vrms-.log", rollingInterval: RollingInterval.Day,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting VehicleRent Pro...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File("Logs/vrms-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                       sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
    {
        o.Password.RequiredLength         = 6;
        o.Password.RequireUppercase       = true;
        o.Password.RequireDigit           = true;
        o.Password.RequireNonAlphanumeric = false;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
        o.User.RequireUniqueEmail         = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(o =>
    {
        o.LoginPath         = "/Account/Login";
        o.AccessDeniedPath  = "/Account/AccessDenied";
        o.ExpireTimeSpan    = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });

    builder.Services.AddControllersWithViews(o =>
        o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()))
        .AddRazorRuntimeCompilation();

    // Repositories
    builder.Services.AddScoped<ILocationRepository,    LocationRepository>();
    builder.Services.AddScoped<IVehicleRepository,     VehicleRepository>();
    builder.Services.AddScoped<ICustomerRepository,    CustomerRepository>();
    builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

    // Services
    builder.Services.AddScoped<ILocationService,    LocationService>();
    builder.Services.AddScoped<IVehicleService,     VehicleService>();
    builder.Services.AddScoped<ICustomerService,    CustomerService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();

    builder.Services.AddResponseCaching();
    builder.Services.AddResponseCompression();

    var app = builder.Build();

    app.UseGlobalExceptionHandling();

    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseResponseCaching();

    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

    await SeedAsync(app);
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Fatal startup error."); }
finally               { Log.CloseAndFlush(); }

static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var um          = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rm          = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await db.Database.MigrateAsync();

    foreach (var role in new[] { "Admin", "Staff" })
        if (!await rm.RoleExistsAsync(role))
            await rm.CreateAsync(new IdentityRole(role));

    const string adminEmail = "admin@vrms.lk";
    if (await um.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName       = adminEmail, Email = adminEmail,
            FullName       = "System Administrator",
            IsActive       = true, EmailConfirmed = true
        };
        var r = await um.CreateAsync(admin, "Admin@12345");
        if (r.Succeeded) await um.AddToRoleAsync(admin, "Admin");
    }

    if (!db.Locations.IgnoreQueryFilters().Any())
    {
        db.Locations.AddRange(
            new Location { Address = "No. 1, Galle Road, Colombo 03", ContactNumber = "+94112345678" },
            new Location { Address = "No. 45, Kandy Road, Kadawatha",  ContactNumber = "+94112876543" }
        );
        await db.SaveChangesAsync();
    }
}
