using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RideLab.Models.ViewModels;

public class ObdSessionUploadViewModel
{
    [Display(Name = "Bike")]
    [Required]
    public int BikeId { get; set; }

    [Display(Name = "Session date")]
    [DataType(DataType.Date)]
    public DateTime? SessionDate { get; set; }

    [Display(Name = "OBD file")]
    [DataType(DataType.Upload)]
    public IFormFile? TelemetryFile { get; set; }

    [StringLength(256)]
    public string? Notes { get; set; }
}
