using System.ComponentModel.DataAnnotations;

namespace RideLab.Models;

public class DtcCode
{
    public int Id { get; set; }

    [Required]
    [StringLength(16)]
    public string Code { get; set; } = string.Empty;

    [StringLength(256)]
    public string? Description { get; set; }

    [StringLength(32)]
    public string? Severity { get; set; }

    [StringLength(512)]
    public string? Recommendation { get; set; }

    public ICollection<BikeDtc> Bikes { get; set; } = new List<BikeDtc>();
}
