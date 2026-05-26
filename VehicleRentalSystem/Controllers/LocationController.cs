using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Controllers;

[Authorize]
public class LocationController : Controller
{
    private readonly ILocationService _service;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService service, ILogger<LocationController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // GET: /Location
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        var locations = string.IsNullOrWhiteSpace(search)
            ? await _service.GetAllAsync()
            : await _service.SearchAsync(search);
        return View(locations);
    }

    // GET: /Location/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var location = await _service.GetByIdAsync(id);
        if (location is null) return NotFound();
        return View(location);
    }

    // GET: /Location/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new LocationViewModel());

    // POST: /Location/Create
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(LocationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _service.CreateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        return View(model);
    }

    // GET: /Location/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var location = await _service.GetByIdAsync(id);
        if (location is null) return NotFound();

        var model = new LocationViewModel
        {
            LocationID    = location.LocationID,
            Address       = location.Address,
            ContactNumber = location.ContactNumber
        };
        return View(model);
    }

    // POST: /Location/Edit
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(LocationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _service.UpdateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        return View(model);
    }

    // GET: /Location/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _service.GetByIdAsync(id);
        if (location is null) return NotFound();
        return View(location);
    }

    // POST: /Location/Delete
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Delete), new { id });
    }
}
