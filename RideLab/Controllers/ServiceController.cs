using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RideLab.Data;
using RideLab.Models;

namespace RideLab.Controllers;

[Authorize]
public class ServiceController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var reminders = await _context.ServiceReminders
            .Include(r => r.Bike)
            .OrderBy(r => r.IsCompleted)
            .ThenBy(r => r.DueDate)
            .AsNoTracking()
            .ToListAsync();
        return View(reminders);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reminder = await _context.ServiceReminders
            .Include(r => r.Bike)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return reminder == null ? NotFound() : View(reminder);
    }

    public async Task<IActionResult> Create(int? bikeId)
    {
        var defaultId = await PopulateBikesAsync(bikeId);
        return View(new ServiceReminder
        {
            BikeId = defaultId ?? 0,
            DueDate = DateTime.UtcNow.AddDays(7)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BikeId,Title,Description,DueDate,DueMileage")] ServiceReminder reminder)
    {
        if (!await _context.Bikes.AnyAsync(b => b.Id == reminder.BikeId))
        {
            ModelState.AddModelError(nameof(reminder.BikeId), "Please select a valid bike.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateBikesAsync(reminder.BikeId);
            return View(reminder);
        }

        reminder.CreatedAtUtc = DateTime.UtcNow;
        _context.ServiceReminders.Add(reminder);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = reminder.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reminder = await _context.ServiceReminders.FindAsync(id);
        if (reminder == null)
        {
            return NotFound();
        }

        await PopulateBikesAsync(reminder.BikeId);
        return View(reminder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BikeId,Title,Description,DueDate,DueMileage,IsCompleted,CreatedAtUtc")] ServiceReminder reminder)
    {
        if (id != reminder.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateBikesAsync(reminder.BikeId);
            return View(reminder);
        }

        if (!await _context.Bikes.AsNoTracking().AnyAsync(b => b.Id == reminder.BikeId))
        {
            ModelState.AddModelError(nameof(reminder.BikeId), "Please select a valid bike.");
            await PopulateBikesAsync(reminder.BikeId);
            return View(reminder);
        }

        _context.ServiceReminders.Update(reminder);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = reminder.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var reminder = await _context.ServiceReminders.FindAsync(id);
        if (reminder == null)
        {
            return NotFound();
        }

        reminder.IsCompleted = true;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reminder = await _context.ServiceReminders.Include(r => r.Bike).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return reminder == null ? NotFound() : View(reminder);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reminder = await _context.ServiceReminders.FindAsync(id);
        if (reminder != null)
        {
            _context.ServiceReminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> PopulateBikesAsync(int? selectedId = null)
    {
        var bikes = await _context.Bikes.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        var defaultId = selectedId ?? bikes.FirstOrDefault()?.Id;
        ViewBag.BikeId = new SelectList(bikes, nameof(Bike.Id), nameof(Bike.Name), defaultId);
        return defaultId;
    }
}
