using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models;

/// <summary>
/// Represents a physical rental location/branch.
/// </summary>
[Table("Location")]
public class Location
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LocationID { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required.")]
    [StringLength(20, MinimumLength = 7, ErrorMessage = "Contact number must be between 7 and 20 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
