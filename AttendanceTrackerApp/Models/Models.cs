using System.ComponentModel.DataAnnotations;

namespace AttendanceTrackerApp.Models;

public class EventItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}

public class Attendee
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EmployeeId { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int AttendeeId { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    public EventItem? Event { get; set; }
    public Attendee? Attendee { get; set; }
}

public class AttendanceScanRequest
{
    public string QrCode { get; set; } = string.Empty;
}
