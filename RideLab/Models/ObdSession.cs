using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class ObdSession
{
    public int Id { get; set; }

    [Display(Name = "Bike")]
    public int BikeId { get; set; }

    public Bike? Bike { get; set; }

    [Display(Name = "Session date")]
    public DateTime SessionDateUtc { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? SourceFileName { get; set; }

    [StringLength(256)]
    public string? StoredFilePath { get; set; }

    [StringLength(256)]
    public string? Notes { get; set; }

    [Display(Name = "Average RPM")]
    public double? AverageRpm { get; set; }

    [Display(Name = "Max throttle %")]
    public double? MaxThrottlePosition { get; set; }

    [Display(Name = "Detected anomalies")]
    public string? AnomalySummary { get; set; }

    public ICollection<ObdDataPoint> DataPoints { get; set; } = new List<ObdDataPoint>();
}
