using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideLab.Data;
using RideLab.Models;

namespace RideLab.Controllers;

[Authorize]
public class BikeController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var bikes = await _context.Bikes
            .Include(b => b.ObdSessions)
            .Include(b => b.ServiceReminders)
            .AsNoTracking()
            .ToListAsync();

        return View(bikes);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bike = await _context.Bikes
            .Include(b => b.ObdSessions)
            .Include(b => b.ActiveDtcs)
                .ThenInclude(d => d.DtcCode)
            .Include(b => b.ServiceReminders)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return bike == null ? NotFound() : View(bike);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Manufacturer,Model,Year,Vin,Engine,Notes")] Bike bike)
    {
        if (!ModelState.IsValid)
        {
            return View(bike);
        }

        bike.CreatedAtUtc = DateTime.UtcNow;
        _context.Add(bike);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bike = await _context.Bikes.FindAsync(id);
        return bike == null ? NotFound() : View(bike);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Manufacturer,Model,Year,Vin,Engine,Notes,CreatedAtUtc")] Bike bike)
    {
        if (id != bike.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(bike);
        }

        try
        {
            _context.Update(bike);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BikeExists(bike.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToAction(nameof(Details), new { id = bike.Id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bike = await _context.Bikes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return bike == null ? NotFound() : View(bike);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bike = await _context.Bikes.FindAsync(id);
        if (bike != null)
        {
            _context.Bikes.Remove(bike);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private Task<bool> BikeExists(int id)
    {
        return _context.Bikes.AnyAsync(e => e.Id == id);
    }
}
