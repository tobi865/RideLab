using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class BikeDtc
{
    public int BikeId { get; set; }

    public Bike? Bike { get; set; }

    public int DtcCodeId { get; set; }

    public DtcCode? DtcCode { get; set; }

    [Display(Name = "Detected at")]
    public DateTime DetectedAtUtc { get; set; } = DateTime.UtcNow;

    [Display(Name = "Resolved")]
    public bool IsResolved { get; set; }

    [StringLength(256)]
    public string? Notes { get; set; }
}
