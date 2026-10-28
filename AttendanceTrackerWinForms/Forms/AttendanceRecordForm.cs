using System.Data;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Diagnostics;
using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class MainForm : Form
{
    private readonly UserAccount _currentUser;
    private readonly Label lblWelcome = new();
    private readonly Button btnEvents = new();
    private readonly Button btnAttendees = new();
    private readonly Button btnAttendance = new();
    private readonly Button btnUsers = new();
    private readonly Button btnLogout = new();
    private readonly DateTimePicker dtpFrom = new();
    private readonly DateTimePicker dtpTo = new();
    private readonly Button btnApplyFilter = new();
    private readonly Button btnExportExcel = new();
    private readonly Button btnExportPdf = new();
    private readonly DataGridView dgvRecords = new();

    public MainForm(UserAccount currentUser)
    {
        _currentUser = currentUser;

        Text = "Attendance Tracker";
        Width = 1040;
        Height = 620;
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

        btnUsers.Text = "User Roles";
        btnUsers.Location = new Point(540, 60);
        btnUsers.Size = new Size(140, 35);
        btnUsers.Click += (_, _) => ShowUserRoleForm();
        btnUsers.Visible = _currentUser.Role == "Admin";

        btnLogout.Text = "Logout";
        btnLogout.Location = new Point(930, 60);
        btnLogout.Size = new Size(90, 35);
        btnLogout.Click += (_, _) => Logout();

        var lblFrom = new Label { Text = "From", Location = new Point(20, 110), Size = new Size(40, 20) };
        dtpFrom.Location = new Point(70, 106);
        dtpFrom.Size = new Size(130, 28);

        var lblTo = new Label { Text = "To", Location = new Point(220, 110), Size = new Size(30, 20) };
        dtpTo.Location = new Point(255, 106);
        dtpTo.Size = new Size(130, 28);

        btnApplyFilter.Text = "Apply";
        btnApplyFilter.Location = new Point(400, 104);
        btnApplyFilter.Size = new Size(90, 30);
        btnApplyFilter.Click += (_, _) => LoadDashboard();

        btnExportExcel.Text = "Export Excel";
        btnExportExcel.Location = new Point(510, 104);
        btnExportExcel.Size = new Size(120, 30);
        btnExportExcel.Click += (_, _) => ExportExcel();

        btnExportPdf.Text = "Export PDF";
        btnExportPdf.Location = new Point(645, 104);
        btnExportPdf.Size = new Size(120, 30);
        btnExportPdf.Click += (_, _) => ExportPdf();

        dgvRecords.Location = new Point(20, 150);
        dgvRecords.Size = new Size(980, 400);
        dgvRecords.ReadOnly = true;
        dgvRecords.AllowUserToAddRows = false;
        dgvRecords.AutoGenerateColumns = true;
        dgvRecords.RowHeadersVisible = false;
        dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(lblWelcome);
        Controls.Add(btnEvents);
        Controls.Add(btnAttendees);
        Controls.Add(btnAttendance);
        Controls.Add(btnUsers);
        Controls.Add(btnLogout);
        Controls.Add(lblFrom);
        Controls.Add(dtpFrom);
        Controls.Add(lblTo);
        Controls.Add(dtpTo);
        Controls.Add(btnApplyFilter);
        Controls.Add(btnExportExcel);
        Controls.Add(btnExportPdf);
        Controls.Add(dgvRecords);

        dtpFrom.Value = DateTime.Now.AddDays(-30);
        dtpTo.Value = DateTime.Now;

        LoadDashboard();
    }

    private void LoadDashboard()
    {
        var table = DatabaseHelper.GetAttendanceRecords(dtpFrom.Value, dtpTo.Value);
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

    private void ShowUserRoleForm()
    {
        if (_currentUser.Role != "Admin")
        {
            MessageBox.Show("Only administrators can manage user roles.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var form = new UserRoleForm();
        form.ShowDialog(this);
    }

    private void ExportExcel()
    {
        var table = (DataTable)dgvRecords.DataSource;
        if (table == null || table.Rows.Count == 0)
        {
            MessageBox.Show("No records available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"attendance-report-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");
        var columns = table.Columns;

        for (int i = 0; i < columns.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = columns[i].ColumnName;
        }

        for (int row = 0; row < table.Rows.Count; row++)
        {
            for (int col = 0; col < columns.Count; col++)
            {
                worksheet.Cell(row + 2, col + 1).Value = table.Rows[row][col]?.ToString();
            }
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(saveDialog.FileName);
        MessageBox.Show("Excel export completed.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportPdf()
    {
        var table = (DataTable)dgvRecords.DataSource;
        if (table == null || table.Rows.Count == 0)
        {
            MessageBox.Show("No records available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "PDF File (*.pdf)|*.pdf",
            FileName = $"attendance-report-{DateTime.Now:yyyyMMddHHmmss}.pdf"
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var rows = table.Rows.Cast<DataRow>().Select(row => new
        {
            EventName = row[1]?.ToString() ?? string.Empty,
            AttendeeName = row[2]?.ToString() ?? string.Empty,
            CheckInTime = row[3]?.ToString() ?? string.Empty,
            Notes = row[4]?.ToString() ?? string.Empty
        }).ToList();

        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Attendance Report").SemiBold().FontSize(20);

                page.Content().Table(tableContent =>
                {
                    tableContent.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    tableContent.Header(header =>
                    {
                        header.Cell().Text("Event");
                        header.Cell().Text("Employee");
                        header.Cell().Text("Date/Time");
                        header.Cell().Text("Notes");
                    });

                    foreach (var row in rows)
                    {
                        tableContent.Cell().Text(row.EventName);
                        tableContent.Cell().Text(row.AttendeeName);
                        tableContent.Cell().Text(row.CheckInTime);
                        tableContent.Cell().Text(row.Notes);
                    }
                });
            });
        }).GenerateToFile(saveDialog.FileName);

        MessageBox.Show("PDF export completed.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Logout()
    {
        var login = new LoginForm();
        login.Show();
        Close();
    }
}
