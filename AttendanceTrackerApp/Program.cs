using AttendanceTrackerApp.Data;
using AttendanceTrackerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Admin", "Manager", "Employee" };
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    const string adminEmail = "admin@attendance.local";
    const string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
