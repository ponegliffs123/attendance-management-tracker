using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace AttendanceTrackerApp.Controllers;

public class EventController : Controller
{
    private readonly AppDbContext _db;

    public EventController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _db.Events
            .Include(e => e.AttendanceRecords)
            .ThenInclude(a => a.Attendee)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

        return View(events);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventItem model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        _db.Events.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var eventItem = await _db.Events
            .Include(e => e.AttendanceRecords)
            .ThenInclude(a => a.Attendee)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return View(eventItem);
    }

    public async Task<IActionResult> GenerateQr(int id)
    {
        var eventItem = await _db.Events.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        var payload = $"event:{eventItem.Id}";
        return GenerateQrImage(payload);
    }

    private FileContentResult GenerateQrImage(string payload)
    {
        var generator = new QRCodeGenerator();
        var qrCodeData = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(qrCodeData);
        var imageBytes = qr.GetGraphic(20);

        return File(imageBytes, "image/png");
    }
}
