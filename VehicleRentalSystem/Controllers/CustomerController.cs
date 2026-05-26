using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Interfaces;
using VehicleRentalSystem.ViewModels;

namespace VehicleRentalSystem.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerService _service;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService service, ILogger<CustomerController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // GET: /Customer
    public async Task<IActionResult> Index(string? nic, string? name, string? mobile)
    {
        var vm = new CustomerSearchViewModel
        {
            NIC     = nic,
            Name    = name,
            Mobile  = mobile,
            Results = await _service.SearchAsync(nic, name, mobile)
        };
        return View(vm);
    }

    // GET: /Customer/Details/{nic}
    public async Task<IActionResult> Details(string id)
    {
        var customer = await _service.GetByNicAsync(id);
        if (customer is null) return NotFound();
        return View(customer);
    }

    // GET: /Customer/Create
    public IActionResult Create() => View(new CustomerViewModel());

    // POST: /Customer/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
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

    // GET: /Customer/Edit/{nic}
    public async Task<IActionResult> Edit(string id)
    {
        var customer = await _service.GetByNicAsync(id);
        if (customer is null) return NotFound();

        var vm = new CustomerViewModel
        {
            OriginalNIC       = customer.NIC,
            NIC               = customer.NIC,
            Name              = customer.Name,
            Address           = customer.Address,
            MobilePhoneNumber = customer.MobilePhoneNumber,
            Email             = customer.Email
        };
        return View(vm);
    }

    // POST: /Customer/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerViewModel model)
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

    // GET: /Customer/Delete/{nic}
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var customer = await _service.GetByNicAsync(id);
        if (customer is null) return NotFound();
        return View(customer);
    }

    // POST: /Customer/Delete
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(string id)
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
