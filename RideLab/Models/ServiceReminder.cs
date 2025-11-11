using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class ServiceReminder
{
    public int Id { get; set; }

    [Display(Name = "Bike")]
    public int BikeId { get; set; }

    public Bike? Bike { get; set; }

    [Required]
    [StringLength(128)]
    public string Title { get; set; } = string.Empty;

    [StringLength(256)]
    public string? Description { get; set; }

    [Display(Name = "Due date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Due mileage")]
    public int? DueMileage { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
