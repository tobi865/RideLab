using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class ObdDataPoint
{
    public int Id { get; set; }

    public int ObdSessionId { get; set; }

    public ObdSession? ObdSession { get; set; }

    [Required]
    [StringLength(64)]
    public string Metric { get; set; } = string.Empty;

    [Display(Name = "Value")]
    public double Value { get; set; }

    [Display(Name = "Recorded at")]
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
