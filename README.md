# Attendance Management Tracker

A simple ASP.NET Core C# application for managing attendance events, employees, and QR-based attendance tracking.

## Features

- Create attendance events
- Save attendee records
- Generate QR codes for attendance checks
- Scan QR codes using a webcam or uploaded image
- Save attendance records automatically
- View attendance history

## Tech Stack

- ASP.NET Core MVC
- C#
- SQLite database
- Entity Framework Core
- QRCoder
- html5-qrcode (browser scanner)

## Setup

1. Install .NET 8 SDK
2. In the project directory, run:

```bash
dotnet restore
dotnet run
```

3. Open the app in the browser:

```text
https://localhost:5001
```

## Usage

1. Create an event from the dashboard
2. Add attendees
3. Open the Attendance page
4. Select an event and attendee
5. Generate the attendee QR code
6. Scan the QR code using the built-in scanner
7. The attendance record is saved automatically

## Notes

This project is a starter attendance system intended for local development and demo use.
