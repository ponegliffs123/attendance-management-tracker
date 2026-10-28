using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTrackerApp.Controllers;

public class AttendeeController : Controller
{
    private readonly AppDbContext _db;

    public AttendeeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var attendees = await _db.Attendees.OrderBy(a => a.FullName).ToListAsync();
        return View(attendees);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Attendee model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Attendees.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Attendee saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}
