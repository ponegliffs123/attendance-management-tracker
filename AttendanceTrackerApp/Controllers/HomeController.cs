using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTrackerApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _db.Events.OrderByDescending(e => e.EventDate).ToListAsync();
        var attendees = await _db.Attendees.OrderBy(a => a.FullName).ToListAsync();
        var recentAttendance = await _db.AttendanceRecords
            .Include(a => a.Attendee)
            .Include(a => a.Event)
            .OrderByDescending(a => a.CheckInTime)
            .Take(10)
            .ToListAsync();

        var dashboard = new DashboardViewModel
        {
            Events = events,
            Attendees = attendees,
            RecentAttendance = recentAttendance
        };

        return View(dashboard);
    }
}

public class DashboardViewModel
{
    public List<EventItem> Events { get; set; } = new();
    public List<Attendee> Attendees { get; set; } = new();
    public List<AttendanceRecord> RecentAttendance { get; set; } = new();
}
