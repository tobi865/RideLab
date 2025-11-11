using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class Bike
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Manufacturer { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }

    [Range(1970, 2100)]
    public int? Year { get; set; }

    [StringLength(100)]
    public string? Vin { get; set; }

    [StringLength(100)]
    public string? Engine { get; set; }

    [StringLength(256)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ObdSession> ObdSessions { get; set; } = new List<ObdSession>();

    public ICollection<ServiceReminder> ServiceReminders { get; set; } = new List<ServiceReminder>();

    public ICollection<BikeDtc> ActiveDtcs { get; set; } = new List<BikeDtc>();
}
