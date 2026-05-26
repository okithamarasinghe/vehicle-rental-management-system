using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Interfaces;

namespace VehicleRentalSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IReservationService reservationService, ILogger<HomeController> logger)
    {
        _reservationService = reservationService;
        _logger             = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboard = await _reservationService.GetDashboardDataAsync();
            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard.");
            return View("Error");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
