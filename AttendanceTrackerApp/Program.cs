using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Events.Any())
    {
        db.Events.AddRange(
            new EventItem
            {
                Name = "Weekly Team Meeting",
                Description = "Monthly planning meeting.",
                Location = "Conference Room A",
                EventDate = DateTime.UtcNow.AddDays(2),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventItem
            {
                Name = "Security Training",
                Description = "Workplace safety and security training.",
                Location = "Learning Hall",
                EventDate = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    if (!db.Attendees.Any())
    {
        db.Attendees.AddRange(
            new Attendee { FullName = "Jane Smith", EmployeeId = "EMP-1001", Email = "jane@example.com", IsActive = true },
            new Attendee { FullName = "John Doe", EmployeeId = "EMP-1002", Email = "john@example.com", IsActive = true },
            new Attendee { FullName = "Sarah Johnson", EmployeeId = "EMP-1003", Email = "sarah@example.com", IsActive = true }
        );
    }

    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
