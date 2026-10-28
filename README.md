# Attendance Management Tracker

A C# ASP.NET Core application for managing attendance events, employees, and QR-code based check-ins.

## Features

- Add and manage attendance events
- Register employees/attendees
- Generate QR codes for event attendance
- Scan QR codes to record attendance
- View recent attendance records
- SQLite database support for local development

## Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQLite
- QRCoder

## Run locally

1. Install .NET 8 SDK
2. Open the project folder
3. Run:

```bash
dotnet restore
dotnet build
dotnet run
```

4. Open the app in the browser:

```text
https://localhost:5001
```

## Notes

This starter project is designed for attendance tracking in a school, office, or event environment.
