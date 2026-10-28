namespace AttendanceTrackerWinForms;

public class UserAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
}

public class EventItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}

public class Attendee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int AttendeeId { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.Now;
    public string Notes { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string AttendeeName { get; set; } = string.Empty;
}
