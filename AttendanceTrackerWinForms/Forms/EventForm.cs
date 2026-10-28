using System.Data;
using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class MainForm : Form
{
    private readonly UserAccount _currentUser;
    private readonly Label lblWelcome = new();
    private readonly Button btnEvents = new();
    private readonly Button btnAttendees = new();
    private readonly Button btnAttendance = new();
    private readonly Button btnLogout = new();
    private readonly DataGridView dgvRecords = new();

    public MainForm(UserAccount currentUser)
    {
        _currentUser = currentUser;

        Text = "Attendance Tracker";
        Width = 980;
        Height = 560;
        StartPosition = FormStartPosition.CenterScreen;

        lblWelcome.Text = $"Welcome, {_currentUser.FullName} ({_currentUser.Role})";
        lblWelcome.Location = new Point(20, 20);
        lblWelcome.Size = new Size(500, 28);
        lblWelcome.Font = new Font("Segoe UI", 12, FontStyle.Bold);

        btnEvents.Text = "Manage Events";
        btnEvents.Location = new Point(20, 60);
        btnEvents.Size = new Size(140, 35);
        btnEvents.Click += (_, _) => ShowEventForm();

        btnAttendees.Text = "Manage Attendees";
        btnAttendees.Location = new Point(180, 60);
        btnAttendees.Size = new Size(160, 35);
        btnAttendees.Click += (_, _) => ShowAttendeeForm();

        btnAttendance.Text = "Attendance Log";
        btnAttendance.Location = new Point(360, 60);
        btnAttendance.Size = new Size(160, 35);
        btnAttendance.Click += (_, _) => ShowAttendanceForm();

        btnLogout.Text = "Logout";
        btnLogout.Location = new Point(860, 60);
        btnLogout.Size = new Size(90, 35);
        btnLogout.Click += (_, _) => Logout();

        dgvRecords.Location = new Point(20, 110);
        dgvRecords.Size = new Size(930, 390);
        dgvRecords.ReadOnly = true;
        dgvRecords.AllowUserToAddRows = false;
        dgvRecords.AutoGenerateColumns = true;
        dgvRecords.RowHeadersVisible = false;
        dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(lblWelcome);
        Controls.Add(btnEvents);
        Controls.Add(btnAttendees);
        Controls.Add(btnAttendance);
        Controls.Add(btnLogout);
        Controls.Add(dgvRecords);

        LoadDashboard();
    }

    private void LoadDashboard()
    {
        var table = DatabaseHelper.GetAttendanceRecords();
        dgvRecords.DataSource = table;
    }

    private void ShowEventForm()
    {
        var form = new EventForm();
        form.ShowDialog(this);
        LoadDashboard();
    }

    private void ShowAttendeeForm()
    {
        var form = new AttendeeForm();
        form.ShowDialog(this);
        LoadDashboard();
    }

    private void ShowAttendanceForm()
    {
        var form = new AttendanceRecordForm();
        form.ShowDialog(this);
        LoadDashboard();
    }

    private void Logout()
    {
        var login = new LoginForm();
        login.Show();
        Close();
    }
}
