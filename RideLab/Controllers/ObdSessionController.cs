using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RideLab.Data;
using RideLab.Models;
using RideLab.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

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

        // 🔍 Auto-analyze telemetry file
        if (!string.IsNullOrEmpty(storedPath))
        {
            var physicalPath = Path.Combine(_environment.WebRootPath, storedPath.TrimStart('/'));
            var dataPoints = await ParseTelemetryFile(physicalPath, session.Id);

            if (dataPoints.Any())
            {
                _context.ObdDataPoints.AddRange(dataPoints);

                var rpmValues = dataPoints.Where(d => d.Metric == "RPM").Select(d => d.Value).ToList();
                var throttleValues = dataPoints.Where(d => d.Metric == "Throttle").Select(d => d.Value).ToList();

                session.AverageRpm = rpmValues.Any() ? rpmValues.Average() : null;
                session.MaxThrottlePosition = throttleValues.Any() ? throttleValues.Max() : null;
                session.AnomalySummary = "Analyzed successfully";
                await _context.SaveChangesAsync();
            }
            else
            {
                session.AnomalySummary = "No telemetry data detected";
                await _context.SaveChangesAsync();
            }
        }


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

    private async Task<List<ObdDataPoint>> ParseTelemetryFile(string filePath, int sessionId)
    {
        var points = new List<ObdDataPoint>();

        if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            var jsonText = await System.IO.File.ReadAllTextAsync(filePath);
            using var doc = JsonDocument.Parse(jsonText);

            if (doc.RootElement.TryGetProperty("samples", out var samples))
            {
                foreach (var s in samples.EnumerateArray())
                {
                    var timestamp = DateTime.Parse(s.GetProperty("timestamp").GetString()!);
                    if (s.TryGetProperty("rpm", out var rpm))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "RPM", Value = rpm.GetDouble(), RecordedAtUtc = timestamp });

                    if (s.TryGetProperty("throttle_pct", out var throttle))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "Throttle", Value = throttle.GetDouble(), RecordedAtUtc = timestamp });

                    if (s.TryGetProperty("speed_kmh", out var speed))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "Speed", Value = speed.GetDouble(), RecordedAtUtc = timestamp });
                }
            }
        }
        else if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            var lines = await System.IO.File.ReadAllLinesAsync(filePath);
            if (lines.Length > 1)
            {
                var headers = lines[0].Split(',');
                int timestampIndex = Array.FindIndex(headers, h => h.Trim().Equals("timestamp", StringComparison.OrdinalIgnoreCase));
                int rpmIndex = Array.FindIndex(headers, h => h.Trim().Equals("rpm", StringComparison.OrdinalIgnoreCase));
                int throttleIndex = Array.FindIndex(headers, h => h.Trim().Equals("throttle_pct", StringComparison.OrdinalIgnoreCase));
                int speedIndex = Array.FindIndex(headers, h => h.Trim().Equals("speed_kmh", StringComparison.OrdinalIgnoreCase));

                // ✅ защитна проверка
                if (timestampIndex == -1)
                {
                    Console.WriteLine("⚠️ Missing 'timestamp' column in CSV!");
                    return points;
                }

                foreach (var line in lines.Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length <= timestampIndex) continue;
                    if (!DateTime.TryParse(parts[timestampIndex], out var timestamp)) continue;

                    if (rpmIndex >= 0 && parts.Length > rpmIndex && double.TryParse(parts[rpmIndex], out var rpm))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "RPM", Value = rpm, RecordedAtUtc = timestamp });

                    if (throttleIndex >= 0 && parts.Length > throttleIndex && double.TryParse(parts[throttleIndex], out var throttle))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "Throttle", Value = throttle, RecordedAtUtc = timestamp });

                    if (speedIndex >= 0 && parts.Length > speedIndex && double.TryParse(parts[speedIndex], out var speed))
                        points.Add(new ObdDataPoint { ObdSessionId = sessionId, Metric = "Speed", Value = speed, RecordedAtUtc = timestamp });
                }
            }

        }

        return points;
    }

}
