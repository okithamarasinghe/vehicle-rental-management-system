using System.ComponentModel.DataAnnotations;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.ViewModels;

// ── Location ViewModel ────────────────────────────────────────────────────────
public class LocationViewModel
{
    public int LocationID { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300, MinimumLength = 5)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required.")]
    [StringLength(20, MinimumLength = 7)]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    public int VehicleCount { get; set; }
}

// ── Vehicle ViewModel ─────────────────────────────────────────────────────────
public class VehicleViewModel
{
    [Display(Name = "Plate Number")]
    public string? OriginalPlateNumber { get; set; } // for edit tracking

    [Required(ErrorMessage = "Plate number is required.")]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "Plate Number")]
    public string PlateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manufacturer is required.")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required.")]
    [StringLength(100, MinimumLength = 1)]
    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    [Display(Name = "Year")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Required]
    [Display(Name = "Availability Status")]
    public VehicleAvailabilityStatus AvailabilityStatus { get; set; } = VehicleAvailabilityStatus.Available;

    [Required(ErrorMessage = "Location is required.")]
    [Display(Name = "Location")]
    public int LocationID { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Daily Rate (LKR)")]
    public decimal DailyRate { get; set; }

    // For view display
    public string? LocationAddress  { get; set; }
    public IEnumerable<LocationViewModel> Locations { get; set; } = Enumerable.Empty<LocationViewModel>();
}

// ── Customer ViewModel ────────────────────────────────────────────────────────
public class CustomerViewModel
{
    [Required(ErrorMessage = "NIC is required.")]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "NIC Number")]
    public string NIC { get; set; } = string.Empty;

    [Display(Name = "NIC Number (Original)")]
    public string? OriginalNIC { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(200, MinimumLength = 2)]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300, MinimumLength = 5)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^[\+]?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Invalid mobile number.")]
    [Display(Name = "Mobile Phone Number")]
    public string MobilePhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    public int ReservationCount { get; set; }
}

// ── Reservation ViewModel ─────────────────────────────────────────────────────
public class ReservationViewModel
{
    public int ReservationID { get; set; }

    [Required(ErrorMessage = "Customer is required.")]
    [Display(Name = "Customer NIC")]
    public string CustomerNIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle is required.")]
    [Display(Name = "Vehicle Plate Number")]
    public string PlateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Rental Start Date")]
    public DateTime RentalStartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Rental End Date")]
    public DateTime RentalEndDate { get; set; } = DateTime.Today.AddDays(1);

    [Range(0, double.MaxValue)]
    [Display(Name = "Total Price (LKR)")]
    public decimal Price { get; set; }

    [Display(Name = "Payment Status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // For view display
    public string? CustomerName   { get; set; }
    public string? VehicleDetails { get; set; }
    public decimal DailyRate      { get; set; }
    public int     RentalDays     => RentalEndDate > RentalStartDate
                                     ? (RentalEndDate - RentalStartDate).Days
                                     : 0;

    public IEnumerable<CustomerViewModel> Customers { get; set; } = Enumerable.Empty<CustomerViewModel>();
    public IEnumerable<VehicleViewModel>  Vehicles  { get; set; } = Enumerable.Empty<VehicleViewModel>();
}

// ── Dashboard ViewModel ───────────────────────────────────────────────────────
public class DashboardViewModel
{
    public int     TotalVehicles       { get; set; }
    public int     AvailableVehicles   { get; set; }
    public int     ReservedVehicles    { get; set; }
    public int     MaintenanceVehicles { get; set; }
    public int     TotalCustomers      { get; set; }
    public int     TotalLocations      { get; set; }
    public int     ActiveReservations  { get; set; }
    public int     TotalReservations   { get; set; }
    public decimal TotalRevenue        { get; set; }
    public decimal MonthRevenue        { get; set; }

    public IList<ReservationListItemViewModel> RecentReservations { get; set; } = new List<ReservationListItemViewModel>();
    public IList<MonthlyChartPoint>            MonthlyRevenue     { get; set; } = new List<MonthlyChartPoint>();
    public IList<MonthlyChartPoint>            MonthlyCount       { get; set; } = new List<MonthlyChartPoint>();
    public IList<LocationVehicleCount>         LocationBreakdown  { get; set; } = new List<LocationVehicleCount>();
}

public class MonthlyChartPoint
{
    public string Label  { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class LocationVehicleCount
{
    public string Location { get; set; } = string.Empty;
    public int    Count    { get; set; }
}

// ── Reservation List Item (lightweight) ──────────────────────────────────────
public class ReservationListItemViewModel
{
    public int    ReservationID   { get; set; }
    public string CustomerName    { get; set; } = string.Empty;
    public string CustomerNIC     { get; set; } = string.Empty;
    public string PlateNumber     { get; set; } = string.Empty;
    public string VehicleDetails  { get; set; } = string.Empty;
    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal Price          { get; set; }
    public PaymentStatus Payment  { get; set; }
    public bool IsActive          { get; set; }
}

// ── Search ViewModels ─────────────────────────────────────────────────────────
public class VehicleSearchViewModel
{
    public string? PlateNumber    { get; set; }
    public string? Manufacturer   { get; set; }
    public string? Model          { get; set; }
    public VehicleAvailabilityStatus? Status { get; set; }
    public IEnumerable<Vehicle> Results { get; set; } = Enumerable.Empty<Vehicle>();
}

public class CustomerSearchViewModel
{
    public string? NIC    { get; set; }
    public string? Name   { get; set; }
    public string? Mobile { get; set; }
    public IEnumerable<Customer> Results { get; set; } = Enumerable.Empty<Customer>();
}

public class ReservationSearchViewModel
{
    public int?      ReservationID { get; set; }
    public string?   CustomerNIC   { get; set; }
    public string?   PlateNumber   { get; set; }
    public DateTime? StartDate     { get; set; }
    public DateTime? EndDate       { get; set; }
    public IEnumerable<Reservation> Results { get; set; } = Enumerable.Empty<Reservation>();
}

// ── Identity ViewModels ───────────────────────────────────────────────────────
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = "Staff";
}
