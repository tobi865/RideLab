using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RideLab.Data;
using RideLab.Models;
using RideLab.Models.ViewModels;

namespace RideLab.Controllers;

[Authorize]
public class ObdSessionController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly IWebHostEnvironment _environment = environment;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var sessions = await _context.ObdSessions
            .Include(s => s.Bike)
            .AsNoTracking()
            .OrderByDescending(s => s.SessionDateUtc)
            .ToListAsync();
        return View(sessions);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var session = await _context.ObdSessions
            .Include(s => s.Bike)
            .Include(s => s.DataPoints)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return session == null ? NotFound() : View(session);
    }

    public async Task<IActionResult> Upload(int? bikeId)
    {
        var defaultId = await PopulateBikesAsync(bikeId);
        return View(new ObdSessionUploadViewModel
        {
            SessionDate = DateTime.UtcNow,
            BikeId = defaultId ?? 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(ObdSessionUploadViewModel model)
    {
        if (!await _context.Bikes.AnyAsync(b => b.Id == model.BikeId))
        {
            ModelState.AddModelError(nameof(model.BikeId), "Please select a valid bike.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateBikesAsync(model.BikeId);
            return View(model);
        }

        string? storedPath = null;
        string? originalName = null;
        if (model.TelemetryFile is { Length: > 0 })
        {
            originalName = model.TelemetryFile.FileName;
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(model.TelemetryFile.FileName)}";
            var filePath = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await model.TelemetryFile.CopyToAsync(stream);
            storedPath = $"/uploads/{fileName}";
        }

        var session = new ObdSession
        {
            BikeId = model.BikeId,
            SessionDateUtc = model.SessionDate ?? DateTime.UtcNow,
            Notes = model.Notes,
            SourceFileName = originalName,
            StoredFilePath = storedPath,
            AverageRpm = null,
            MaxThrottlePosition = null,
            AnomalySummary = "Pending analysis"
        };

        _context.ObdSessions.Add(session);
        await _context.SaveChangesAsync();

        TempData["Message"] = "OBD session uploaded successfully. Run the analysis job to populate metrics.";
        return RedirectToAction(nameof(Details), new { id = session.Id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var session = await _context.ObdSessions
            .Include(s => s.Bike)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return session == null ? NotFound() : View(session);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var session = await _context.ObdSessions.FindAsync(id);
        if (session != null)
        {
            _context.ObdSessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> PopulateBikesAsync(int? selectedId = null)
    {
        var bikes = await _context.Bikes
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, Label = b.Name })
            .ToListAsync();
        var defaultId = selectedId ?? bikes.FirstOrDefault()?.Id;
        ViewBag.BikeId = new SelectList(bikes, "Id", "Label", defaultId);
        return defaultId;
    }
}
