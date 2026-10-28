# Attendance Tracker WinForms App

This project is a Windows Forms desktop application for attendance tracking.

## Features

- Login screen
- Role-based access with Admin/Employee support
- Manage events
- Manage attendees
- Record attendance
- View attendance log
- SQL Server database connection

## Default Login

- Username: admin
- Password: admin123

## Run

Open the project in Visual Studio and start debugging.

Alternatively, from the command line:

```bash
dotnet build AttendanceTrackerWinForms/AttendanceTrackerWinForms.csproj
```

Then run the generated executable.

## Database

The app connects to SQL Server using:

```text
Server=(localdb)\MSSQLLocalDB;Database=AttendanceTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;
```

If your SQL Server instance is different, update the connection string in `DatabaseHelper.cs`.

## Notes

This is a desktop version of the attendance tracker and is intended for Windows machines.

