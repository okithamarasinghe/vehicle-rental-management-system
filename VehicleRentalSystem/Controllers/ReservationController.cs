using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Controllers;

[Authorize]
public class ReservationController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly ICustomerService    _customerService;
    private readonly IVehicleService     _vehicleService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(IReservationService reservationService, ICustomerService customerService,
        IVehicleService vehicleService, ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _customerService    = customerService;
        _vehicleService     = vehicleService;
        _logger             = logger;
    }

    // GET: /Reservation
    public async Task<IActionResult> Index(int? id, string? nic, string? plate,
        DateTime? startDate, DateTime? endDate)
    {
        var vm = new ReservationSearchViewModel
        {
            ReservationID = id,
            CustomerNIC   = nic,
            PlateNumber   = plate,
            StartDate     = startDate,
            EndDate       = endDate,
            Results       = await _reservationService.SearchAsync(id, nic, plate, startDate, endDate)
        };
        return View(vm);
    }

    // GET: /Reservation/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation is null) return NotFound();
        return View(reservation);
    }

    // GET: /Reservation/Create
    public async Task<IActionResult> Create(string? customerNic)
    {
        var vm = new ReservationViewModel
        {
            CustomerNIC     = customerNic ?? string.Empty,
            RentalStartDate = DateTime.Today,
            RentalEndDate   = DateTime.Today.AddDays(1),
            Customers       = await GetCustomerSelectListAsync(),
            Vehicles        = await GetVehicleSelectListAsync()
        };
        return View(vm);
    }

    // POST: /Reservation/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Customers = await GetCustomerSelectListAsync();
            model.Vehicles  = await GetVehicleSelectListAsync();
            return View(model);
        }

        var result = await _reservationService.CreateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        model.Customers = await GetCustomerSelectListAsync();
        model.Vehicles  = await GetVehicleSelectListAsync();
        return View(model);
    }

    // GET: /Reservation/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _reservationService.GetByIdAsync(id);
        if (r is null) return NotFound();

        var vm = new ReservationViewModel
        {
            ReservationID   = r.ReservationID,
            CustomerNIC     = r.CustomerNIC,
            PlateNumber     = r.PlateNumber,
            RentalStartDate = r.RentalStartDate,
            RentalEndDate   = r.RentalEndDate,
            Price           = r.Price,
            PaymentStatus   = r.PaymentStatus,
            Notes           = r.Notes,
            Customers       = await GetCustomerSelectListAsync(),
            Vehicles        = await GetVehicleSelectListAsync()
        };
        return View(vm);
    }

    // POST: /Reservation/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReservationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Customers = await GetCustomerSelectListAsync();
            model.Vehicles  = await GetVehicleSelectListAsync();
            return View(model);
        }

        var result = await _reservationService.UpdateAsync(model);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.Message);
        model.Customers = await GetCustomerSelectListAsync();
        model.Vehicles  = await GetVehicleSelectListAsync();
        return View(model);
    }

    // GET: /Reservation/Cancel/5
    public async Task<IActionResult> Cancel(int id)
    {
        var r = await _reservationService.GetByIdAsync(id);
        if (r is null) return NotFound();
        return View(r);
    }

    // POST: /Reservation/Cancel
    [HttpPost, ActionName("Cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        var result = await _reservationService.CancelAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Reservation/UpdatePayment (AJAX)
    [HttpPost]
    public async Task<IActionResult> UpdatePayment(int id, PaymentStatus status)
    {
        var result = await _reservationService.UpdatePaymentStatusAsync(id, status);
        return Json(new { success = result.Success, message = result.Message });
    }

    // GET: /Reservation/History/{nic}
    public async Task<IActionResult> History(string id)
    {
        var reservations = await _reservationService.GetByCustomerAsync(id);
        ViewBag.CustomerNIC = id;
        return View(reservations);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<IEnumerable<CustomerViewModel>> GetCustomerSelectListAsync()
    {
        var customers = await _customerService.GetAllAsync();
        return customers.Select(c => new CustomerViewModel
        {
            NIC  = c.NIC,
            Name = c.Name,
            MobilePhoneNumber = c.MobilePhoneNumber
        });
    }

    private async Task<IEnumerable<VehicleViewModel>> GetVehicleSelectListAsync()
    {
        var vehicles = await _vehicleService.GetAvailableAsync();
        return vehicles.Select(v => new VehicleViewModel
        {
            PlateNumber  = v.PlateNumber,
            Manufacturer = v.Manufacturer,
            Model        = v.Model,
            Year         = v.Year,
            DailyRate    = v.DailyRate
        });
    }
}
