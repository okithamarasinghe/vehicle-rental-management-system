using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models;

/// <summary>
/// Represents a customer. NIC is the natural primary key.
/// </summary>
[Table("Customer")]
public class Customer
{
    [Key]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "NIC must be between 5 and 20 characters.")]
    [Required(ErrorMessage = "NIC is required.")]
    [Display(Name = "NIC Number")]
    public string NIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile phone number is required.")]
    [StringLength(20, MinimumLength = 7, ErrorMessage = "Mobile number must be between 7 and 20 characters.")]
    [RegularExpression(@"^[\+]?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Invalid mobile phone number.")]
    [Display(Name = "Mobile Phone Number")]
    public string MobilePhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(200)]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
