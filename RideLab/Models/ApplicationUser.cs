using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RideLab.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(64)]
    public string? DisplayName { get; set; }

    [StringLength(128)]
    public string? PreferredWorkshop { get; set; }
}
