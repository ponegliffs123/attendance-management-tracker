using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace AttendanceTrackerApp.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly AppDbContext _db;

    public AttendanceController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AttendanceDashboardViewModel
        {
            Events = await _db.Events.OrderByDescending(e => e.EventDate).ToListAsync(),
            Attendees = await _db.Attendees.OrderBy(a => a.FullName).ToListAsync(),
            AttendanceRecords = await _db.AttendanceRecords
                .Include(a => a.Attendee)
                .Include(a => a.Event)
                .OrderByDescending(a => a.CheckInTime)
                .Take(20)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> GenerateAttendeeQr(int eventId, int attendeeId)
    {
        var eventItem = await _db.Events.FindAsync(eventId);
        var attendee = await _db.Attendees.FindAsync(attendeeId);

        if (eventItem == null || attendee == null)
        {
            return NotFound();
        }

        var payload = $"event:{eventItem.Id}|attendee:{attendee.Id}";
        return GenerateQrImage(payload);
    }

    [HttpPost]
    public async Task<IActionResult> RecordScan([FromBody] AttendanceScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QrCode))
        {
            return BadRequest("QR code is required.");
        }

        var payload = request.QrCode.Trim();
        var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return BadRequest("This QR code is not valid for the attendance system.");
        }

        var eventPart = parts[0];
        var attendeePart = parts[1];

        if (!eventPart.StartsWith("event:") || !attendeePart.StartsWith("attendee:"))
        {
            return BadRequest("The QR code format is invalid.");
        }

        if (!int.TryParse(eventPart.Substring("event:".Length), out var eventId) ||
            !int.TryParse(attendeePart.Substring("attendee:".Length), out var attendeeId))
        {
            return BadRequest("The QR code could not be parsed.");
        }

        var eventItem = await _db.Events.FindAsync(eventId);
        var attendee = await _db.Attendees.FindAsync(attendeeId);

        if (eventItem == null || attendee == null)
        {
            return NotFound("Event or attendee not found.");
        }

        var existingRecord = await _db.AttendanceRecords
            .AnyAsync(ar => ar.EventId == eventId && ar.AttendeeId == attendeeId);

        if (existingRecord)
        {
            return Ok(new { status = "already_logged", message = $"{attendee.FullName} has already checked in for {eventItem.Name}." });
        }

        var attendanceRecord = new AttendanceRecord
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            CheckInTime = DateTime.UtcNow,
            Notes = "QR attendance scan"
        };

        _db.AttendanceRecords.Add(attendanceRecord);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = "success",
            message = $"Attendance saved for {attendee.FullName} at {eventItem.Name}.",
            attendeeName = attendee.FullName,
            eventName = eventItem.Name,
            checkInTime = attendanceRecord.CheckInTime
        });
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

public class AttendanceDashboardViewModel
{
    public List<EventItem> Events { get; set; } = new();
    public List<Attendee> Attendees { get; set; } = new();
    public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
}
