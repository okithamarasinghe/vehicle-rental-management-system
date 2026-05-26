using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models;

public enum VehicleAvailabilityStatus : byte
{
    Available        = 1,
    Reserved         = 2,
    UnderMaintenance = 3
}

/// <summary>
/// Represents a rentable vehicle. Plate number is the natural primary key.
/// </summary>
[Table("Vehicle")]
public class Vehicle
{
    [Key]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Plate number must be between 2 and 20 characters.")]
    [Required(ErrorMessage = "Plate number is required.")]
    [Display(Name = "Plate Number")]
    public string PlateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manufacturer is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Manufacturer must be between 2 and 100 characters.")]
    [Display(Name = "Manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Model must be between 1 and 100 characters.")]
    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Year is required.")]
    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    [Display(Name = "Year")]
    public int Year { get; set; }

    [Required]
    [Display(Name = "Availability Status")]
    public VehicleAvailabilityStatus AvailabilityStatus { get; set; } = VehicleAvailabilityStatus.Available;

    [Required(ErrorMessage = "Location is required.")]
    [Display(Name = "Location")]
    public int LocationID { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Daily rate must be a positive value.")]
    [Display(Name = "Daily Rate (LKR)")]
    public decimal DailyRate { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(LocationID))]
    public virtual Location? Location { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
