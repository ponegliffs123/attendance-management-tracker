using Microsoft.EntityFrameworkCore;
using AttendanceTrackerApp.Models;

namespace AttendanceTrackerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EventItem> Events { get; set; }
    public DbSet<Attendee> Attendees { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.Event)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(ar => ar.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.Attendee)
            .WithMany(a => a.AttendanceRecords)
            .HasForeignKey(ar => ar.AttendeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
