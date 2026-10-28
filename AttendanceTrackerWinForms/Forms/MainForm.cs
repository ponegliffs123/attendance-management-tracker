using System.Data;
using Microsoft.Data.SqlClient;

namespace AttendanceTrackerWinForms;

public static class DatabaseHelper
{
    public static string ConnectionString =>
        "Server=(localdb)\\MSSQLLocalDB;Database=AttendanceTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

    public static void EnsureDatabase()
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using (var cmd = new SqlCommand(
                   @"IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Users (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            Username nvarchar(100) NOT NULL UNIQUE,
                            Password nvarchar(200) NOT NULL,
                            FullName nvarchar(200) NOT NULL,
                            Role nvarchar(50) NOT NULL DEFAULT 'Employee'
                        );
                    END;",
                   conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SqlCommand(
                   @"IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Events (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            Name nvarchar(200) NOT NULL,
                            Description nvarchar(500) NULL,
                            Location nvarchar(200) NULL,
                            EventDate datetime2 NOT NULL,
                            IsActive bit NOT NULL DEFAULT 1
                        );
                    END;",
                   conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SqlCommand(
                   @"IF OBJECT_ID(N'dbo.Attendees', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Attendees (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            FullName nvarchar(200) NOT NULL,
                            EmployeeId nvarchar(100) NOT NULL,
                            Email nvarchar(200) NULL,
                            IsActive bit NOT NULL DEFAULT 1
                        );
                    END;",
                   conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SqlCommand(
                   @"IF OBJECT_ID(N'dbo.AttendanceRecords', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.AttendanceRecords (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            EventId int NOT NULL,
                            AttendeeId int NOT NULL,
                            CheckInTime datetime2 NOT NULL,
                            Notes nvarchar(250) NULL,
                            CONSTRAINT FK_AttendanceRecords_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
                            CONSTRAINT FK_AttendanceRecords_Attendees FOREIGN KEY (AttendeeId) REFERENCES dbo.Attendees(Id) ON DELETE CASCADE
                        );
                    END;",
                   conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users", conn))
        {
            var count = (int)(cmd.ExecuteScalar() ?? 0);
            if (count == 0)
            {
                using var insertCmd = new SqlCommand(
                    @"INSERT INTO dbo.Users (Username, Password, FullName, Role)
                      VALUES (@Username, @Password, @FullName, @Role)", conn);
                insertCmd.Parameters.AddWithValue("@Username", "admin");
                insertCmd.Parameters.AddWithValue("@Password", "admin123");
                insertCmd.Parameters.AddWithValue("@FullName", "System Administrator");
                insertCmd.Parameters.AddWithValue("@Role", "Admin");
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public static UserAccount? ValidateLogin(string username, string password)
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            @"SELECT Id, Username, FullName, Role
              FROM dbo.Users
              WHERE Username = @Username AND Password = @Password",
            conn);

        cmd.Parameters.AddWithValue("@Username", username.Trim());
        cmd.Parameters.AddWithValue("@Password", password);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new UserAccount
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            FullName = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }

    public static DataTable GetEvents()
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            @"SELECT Id, Name, Description, Location, EventDate, IsActive
              FROM dbo.Events
              ORDER BY EventDate DESC",
            conn);

        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetAttendees()
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            @"SELECT Id, FullName, EmployeeId, Email, IsActive
              FROM dbo.Attendees
              ORDER BY FullName",
            conn);

        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetAttendanceRecords(DateTime? from = null, DateTime? to = null)
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        var query = @"SELECT ar.Id, e.Name AS EventName, a.FullName AS AttendeeName, ar.CheckInTime, ar.Notes
                      FROM dbo.AttendanceRecords ar
                      INNER JOIN dbo.Events e ON e.Id = ar.EventId
                      INNER JOIN dbo.Attendees a ON a.Id = ar.AttendeeId
                      WHERE 1 = 1";

        if (from.HasValue)
        {
            query += " AND ar.CheckInTime >= @FromDate";
        }

        if (to.HasValue)
        {
            query += " AND ar.CheckInTime < @ToDate";
        }

        query += " ORDER BY ar.CheckInTime DESC";

        using var cmd = new SqlCommand(query, conn);
        if (from.HasValue)
        {
            cmd.Parameters.AddWithValue("@FromDate", from.Value.Date);
        }

        if (to.HasValue)
        {
            cmd.Parameters.AddWithValue("@ToDate", to.Value.Date.AddDays(1));
        }

        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetUsers()
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            @"SELECT Id, Username, FullName, Role
              FROM dbo.Users
              ORDER BY FullName",
            conn);

        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }

    public static void UpdateUserRole(int userId, string role)
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            @"UPDATE dbo.Users SET Role = @Role WHERE Id = @Id",
            conn);
        cmd.Parameters.AddWithValue("@Id", userId);
        cmd.Parameters.AddWithValue("@Role", role);
        cmd.ExecuteNonQuery();
    }

    public static void SaveEvent(EventItem item)
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        if (item.Id == 0)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.Events (Name, Description, Location, EventDate, IsActive)
                  VALUES (@Name, @Description, @Location, @EventDate, @IsActive)",
                conn);
            cmd.Parameters.AddWithValue("@Name", item.Name);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@Location", item.Location ?? string.Empty);
            cmd.Parameters.AddWithValue("@EventDate", item.EventDate);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            cmd.ExecuteNonQuery();
        }
        else
        {
            using var cmd = new SqlCommand(
                @"UPDATE dbo.Events
                  SET Name = @Name,
                      Description = @Description,
                      Location = @Location,
                      EventDate = @EventDate,
                      IsActive = @IsActive
                  WHERE Id = @Id",
                conn);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.Parameters.AddWithValue("@Name", item.Name);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@Location", item.Location ?? string.Empty);
            cmd.Parameters.AddWithValue("@EventDate", item.EventDate);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            cmd.ExecuteNonQuery();
        }
    }

    public static void SaveAttendee(Attendee item)
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        if (item.Id == 0)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.Attendees (FullName, EmployeeId, Email, IsActive)
                  VALUES (@FullName, @EmployeeId, @Email, @IsActive)",
                conn);
            cmd.Parameters.AddWithValue("@FullName", item.FullName);
            cmd.Parameters.AddWithValue("@EmployeeId", item.EmployeeId);
            cmd.Parameters.AddWithValue("@Email", item.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            cmd.ExecuteNonQuery();
        }
        else
        {
            using var cmd = new SqlCommand(
                @"UPDATE dbo.Attendees
                  SET FullName = @FullName,
                      EmployeeId = @EmployeeId,
                      Email = @Email,
                      IsActive = @IsActive
                  WHERE Id = @Id",
                conn);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.Parameters.AddWithValue("@FullName", item.FullName);
            cmd.Parameters.AddWithValue("@EmployeeId", item.EmployeeId);
            cmd.Parameters.AddWithValue("@Email", item.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            cmd.ExecuteNonQuery();
        }
    }

    public static bool SaveAttendanceRecord(AttendanceRecord record)
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var duplicateCheck = new SqlCommand(
            @"SELECT COUNT(*) FROM dbo.AttendanceRecords WHERE EventId = @EventId AND AttendeeId = @AttendeeId AND CAST(CheckInTime AS date) = CAST(@CheckInTime AS date)",
            conn);
        duplicateCheck.Parameters.AddWithValue("@EventId", record.EventId);
        duplicateCheck.Parameters.AddWithValue("@AttendeeId", record.AttendeeId);
        duplicateCheck.Parameters.AddWithValue("@CheckInTime", record.CheckInTime);

        var exists = (int)(duplicateCheck.ExecuteScalar() ?? 0) > 0;
        if (exists)
        {
            return false;
        }

        using var cmd = new SqlCommand(
            @"INSERT INTO dbo.AttendanceRecords (EventId, AttendeeId, CheckInTime, Notes)
              VALUES (@EventId, @AttendeeId, @CheckInTime, @Notes)",
            conn);
        cmd.Parameters.AddWithValue("@EventId", record.EventId);
        cmd.Parameters.AddWithValue("@AttendeeId", record.AttendeeId);
        cmd.Parameters.AddWithValue("@CheckInTime", record.CheckInTime);
        cmd.Parameters.AddWithValue("@Notes", record.Notes ?? string.Empty);
        cmd.ExecuteNonQuery();
        return true;
    }

    public static DataTable GetEventsForComboBox()
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Name FROM dbo.Events ORDER BY Name", conn);
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetAttendeesForComboBox()
    {
        var table = new DataTable();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, FullName + ' (' + EmployeeId + ')' AS DisplayName FROM dbo.Attendees WHERE IsActive = 1 ORDER BY FullName", conn);
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(table);
        return table;
    }
}
