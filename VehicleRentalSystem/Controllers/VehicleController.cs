using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Controllers;

[Authorize]
public class VehicleController : Controller
{
    private readonly IVehicleService  _vehicleService;
    private readonly ILocationService _locationService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(IVehicleService vehicleService, ILocationService locationService,
        ILogger<VehicleController> logger)
    {
        _vehicleService  = vehicleService;
        _locationService = locationService;
        _logger          = logger;
    }

    // GET: /Vehicle
    public async Task<IActionResult> Index(string? plate, string? manufacturer, string? model,
        VehicleAvailabilityStatus? status)
    {
        var vm = new VehicleSearchViewModel
        {
            PlateNumber  = plate,
            Manufacturer = manufacturer,
            Model        = model,
            Status       = status,
            Results      = await _vehicleService.SearchAsync(plate, manufacturer, model, status)
        };
        return View(vm);
    }

    // GET: /Vehicle/Details/{plate}
    public async Task<IActionResult> Details(string id)
    {
        var vehicle = await _vehicleService.GetByPlateAsync(id);
        if (vehicle is null) return NotFound();
        return View(vehicle);
    }

    // GET: /Vehicle/Create
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var vm = new VehicleViewModel
        {
            Year      = DateTime.Today.Year,
            Locations = await GetLocationSelectListAsync()
        };
        return View(vm);
    }

    // POST: /Vehicle/Create
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(VehicleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Locations = await GetLocationSelectListAsync();
            return View(model);
        }

        var result = await _vehicleService.CreateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        model.Locations = await GetLocationSelectListAsync();
        return View(model);
    }

    // GET: /Vehicle/Edit/{plate}
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var vehicle = await _vehicleService.GetByPlateAsync(id);
        if (vehicle is null) return NotFound();

        var vm = new VehicleViewModel
        {
            OriginalPlateNumber = vehicle.PlateNumber,
            PlateNumber         = vehicle.PlateNumber,
            Manufacturer        = vehicle.Manufacturer,
            Model               = vehicle.Model,
            Year                = vehicle.Year,
            AvailabilityStatus  = vehicle.AvailabilityStatus,
            LocationID          = vehicle.LocationID,
            DailyRate           = vehicle.DailyRate,
            Locations           = await GetLocationSelectListAsync()
        };
        return View(vm);
    }

    // POST: /Vehicle/Edit
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(VehicleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Locations = await GetLocationSelectListAsync();
            return View(model);
        }

        var result = await _vehicleService.UpdateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        model.Locations = await GetLocationSelectListAsync();
        return View(model);
    }

    // GET: /Vehicle/Delete/{plate}
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var vehicle = await _vehicleService.GetByPlateAsync(id);
        if (vehicle is null) return NotFound();
        return View(vehicle);
    }

    // POST: /Vehicle/Delete
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _vehicleService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Delete), new { id });
    }

    // POST: /Vehicle/UpdateStatus  (AJAX)
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(string plateNumber, VehicleAvailabilityStatus status)
    {
        var result = await _vehicleService.UpdateAvailabilityAsync(plateNumber, status);
        return Json(new { success = result.Success, message = result.Message });
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private async Task<IEnumerable<LocationViewModel>> GetLocationSelectListAsync()
    {
        var locations = await _locationService.GetAllAsync();
        return locations.Select(l => new LocationViewModel
        {
            LocationID = l.LocationID,
            Address    = l.Address
        });
    }
}
