using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideLab.Data;
using RideLab.Models;

namespace RideLab.Controllers;

[Authorize]
public class DtcController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? severity)
    {
        var query = _context.DtcCodes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(d => d.Severity == severity);
        }

        var dtcs = await query
            .OrderByDescending(d => d.Severity)
            .ThenBy(d => d.Code)
            .AsNoTracking()
            .ToListAsync();
        return View(dtcs);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dtc = await _context.DtcCodes
            .Include(d => d.Bikes)
                .ThenInclude(b => b.Bike)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return dtc == null ? NotFound() : View(dtc);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Description,Severity,Recommendation")] DtcCode dtc)
    {
        if (!ModelState.IsValid)
        {
            return View(dtc);
        }

        _context.DtcCodes.Add(dtc);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dtc = await _context.DtcCodes.FindAsync(id);
        return dtc == null ? NotFound() : View(dtc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Description,Severity,Recommendation")] DtcCode dtc)
    {
        if (id != dtc.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(dtc);
        }

        _context.Update(dtc);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = dtc.Id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dtc = await _context.DtcCodes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return dtc == null ? NotFound() : View(dtc);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dtc = await _context.DtcCodes.FindAsync(id);
        if (dtc != null)
        {
            _context.DtcCodes.Remove(dtc);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
