using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models;

public enum PaymentStatus : byte
{
    Pending  = 1,
    Paid     = 2,
    Refunded = 3
}

/// <summary>
/// Represents a vehicle rental reservation.
/// Enforces no overlapping reservations at the service layer.
/// </summary>
[Table("Reservation")]
public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Reservation ID")]
    public int ReservationID { get; set; }

    [Required(ErrorMessage = "Customer NIC is required.")]
    [Display(Name = "Customer NIC")]
    public string CustomerNIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle plate number is required.")]
    [Display(Name = "Vehicle Plate Number")]
    public string PlateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rental start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Rental Start Date")]
    public DateTime RentalStartDate { get; set; }

    [Required(ErrorMessage = "Rental end date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Rental End Date")]
    public DateTime RentalEndDate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    [Display(Name = "Total Price (LKR)")]
    public decimal Price { get; set; } = 0;

    [Required]
    [Display(Name = "Payment Status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(CustomerNIC))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(PlateNumber))]
    public virtual Vehicle? Vehicle { get; set; }

    // Computed Properties
    [NotMapped]
    public int RentalDays => (RentalEndDate - RentalStartDate).Days;

    [NotMapped]
    public bool IsActive => !IsDeleted && RentalEndDate >= DateTime.Today;
}
