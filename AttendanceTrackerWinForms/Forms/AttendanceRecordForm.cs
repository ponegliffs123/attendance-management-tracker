using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class AttendeeForm : Form
{
    private readonly TextBox txtFullName = new();
    private readonly TextBox txtEmployeeId = new();
    private readonly TextBox txtEmail = new();
    private readonly CheckBox chkIsActive = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();
    private readonly DataGridView dgvAttendees = new();

    public AttendeeForm()
    {
        Text = "Attendee Maintenance";
        Width = 780;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        var labelName = new Label { Text = "Full Name", Location = new Point(20, 20), Size = new Size(100, 20) };
        txtFullName.Location = new Point(130, 18);
        txtFullName.Size = new Size(240, 28);

        var labelEmployeeId = new Label { Text = "Employee ID", Location = new Point(20, 60), Size = new Size(100, 20) };
        txtEmployeeId.Location = new Point(130, 58);
        txtEmployeeId.Size = new Size(240, 28);

        var labelEmail = new Label { Text = "Email", Location = new Point(20, 100), Size = new Size(100, 20) };
        txtEmail.Location = new Point(130, 98);
        txtEmail.Size = new Size(240, 28);

        chkIsActive.Text = "Active";
        chkIsActive.Location = new Point(130, 140);
        chkIsActive.Size = new Size(120, 24);
        chkIsActive.Checked = true;

        btnSave.Text = "Save";
        btnSave.Location = new Point(130, 180);
        btnSave.Size = new Size(100, 34);
        btnSave.Click += (_, _) => SaveAttendee();

        btnCancel.Text = "Cancel";
        btnCancel.Location = new Point(250, 180);
        btnCancel.Size = new Size(100, 34);
        btnCancel.Click += (_, _) => Close();

        dgvAttendees.Location = new Point(410, 20);
        dgvAttendees.Size = new Size(330, 420);
        dgvAttendees.ReadOnly = true;
        dgvAttendees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvAttendees.CellDoubleClick += (_, _) => LoadSelectedAttendee();

        Controls.Add(labelName);
        Controls.Add(txtFullName);
        Controls.Add(labelEmployeeId);
        Controls.Add(txtEmployeeId);
        Controls.Add(labelEmail);
        Controls.Add(txtEmail);
        Controls.Add(chkIsActive);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        Controls.Add(dgvAttendees);

        LoadAttendees();
    }

    private void LoadAttendees()
    {
        dgvAttendees.DataSource = DatabaseHelper.GetAttendees();
    }

    private void LoadSelectedAttendee()
    {
        if (dgvAttendees.SelectedRows.Count == 0)
        {
            return;
        }

        var row = dgvAttendees.SelectedRows[0];
        txtFullName.Text = row.Cells[1].Value?.ToString() ?? string.Empty;
        txtEmployeeId.Text = row.Cells[2].Value?.ToString() ?? string.Empty;
        txtEmail.Text = row.Cells[3].Value?.ToString() ?? string.Empty;
        chkIsActive.Checked = Convert.ToBoolean(row.Cells[4].Value ?? true);
    }

    private void SaveAttendee()
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmployeeId.Text))
        {
            MessageBox.Show("Full name and employee ID are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var attendee = new Attendee
        {
            FullName = txtFullName.Text.Trim(),
            EmployeeId = txtEmployeeId.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            IsActive = chkIsActive.Checked
        };

        if (dgvAttendees.SelectedRows.Count > 0)
        {
            attendee.Id = Convert.ToInt32(dgvAttendees.SelectedRows[0].Cells[0].Value);
        }

        DatabaseHelper.SaveAttendee(attendee);
        LoadAttendees();
        ClearForm();
        MessageBox.Show("Attendee saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearForm()
    {
        txtFullName.Clear();
        txtEmployeeId.Clear();
        txtEmail.Clear();
        chkIsActive.Checked = true;
        dgvAttendees.ClearSelection();
    }
}
