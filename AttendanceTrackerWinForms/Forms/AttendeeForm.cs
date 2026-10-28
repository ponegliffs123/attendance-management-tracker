using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class EventForm : Form
{
    private readonly TextBox txtName = new();
    private readonly TextBox txtDescription = new();
    private readonly TextBox txtLocation = new();
    private readonly DateTimePicker dtpEventDate = new();
    private readonly CheckBox chkIsActive = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();
    private readonly DataGridView dgvEvents = new();

    public EventForm()
    {
        Text = "Event Maintenance";
        Width = 780;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        var labelName = new Label { Text = "Name", Location = new Point(20, 20), Size = new Size(100, 20) };
        txtName.Location = new Point(130, 18);
        txtName.Size = new Size(240, 28);

        var labelDescription = new Label { Text = "Description", Location = new Point(20, 60), Size = new Size(100, 20) };
        txtDescription.Multiline = true;
        txtDescription.Location = new Point(130, 58);
        txtDescription.Size = new Size(240, 80);

        var labelLocation = new Label { Text = "Location", Location = new Point(20, 150), Size = new Size(100, 20) };
        txtLocation.Location = new Point(130, 148);
        txtLocation.Size = new Size(240, 28);

        var labelDate = new Label { Text = "Event Date", Location = new Point(20, 190), Size = new Size(100, 20) };
        dtpEventDate.Location = new Point(130, 188);
        dtpEventDate.Size = new Size(240, 28);
        dtpEventDate.Value = DateTime.Now;

        chkIsActive.Text = "Active";
        chkIsActive.Location = new Point(130, 228);
        chkIsActive.Size = new Size(120, 24);
        chkIsActive.Checked = true;

        btnSave.Text = "Save";
        btnSave.Location = new Point(130, 270);
        btnSave.Size = new Size(100, 34);
        btnSave.Click += (_, _) => SaveEvent();

        btnCancel.Text = "Cancel";
        btnCancel.Location = new Point(250, 270);
        btnCancel.Size = new Size(100, 34);
        btnCancel.Click += (_, _) => Close();

        dgvEvents.Location = new Point(410, 20);
        dgvEvents.Size = new Size(330, 420);
        dgvEvents.ReadOnly = true;
        dgvEvents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEvents.CellDoubleClick += (_, _) => LoadSelectedEvent();

        Controls.Add(labelName);
        Controls.Add(txtName);
        Controls.Add(labelDescription);
        Controls.Add(txtDescription);
        Controls.Add(labelLocation);
        Controls.Add(txtLocation);
        Controls.Add(labelDate);
        Controls.Add(dtpEventDate);
        Controls.Add(chkIsActive);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        Controls.Add(dgvEvents);

        LoadEvents();
    }

    private void LoadEvents()
    {
        dgvEvents.DataSource = DatabaseHelper.GetEvents();
    }

    private void LoadSelectedEvent()
    {
        if (dgvEvents.SelectedRows.Count == 0)
        {
            return;
        }

        var row = dgvEvents.SelectedRows[0];
        txtName.Text = row.Cells[1].Value?.ToString() ?? string.Empty;
        txtDescription.Text = row.Cells[2].Value?.ToString() ?? string.Empty;
        txtLocation.Text = row.Cells[3].Value?.ToString() ?? string.Empty;
        dtpEventDate.Value = Convert.ToDateTime(row.Cells[4].Value ?? DateTime.Now);
        chkIsActive.Checked = Convert.ToBoolean(row.Cells[5].Value ?? true);
    }

    private void SaveEvent()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Event name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var item = new EventItem
        {
            Name = txtName.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            Location = txtLocation.Text.Trim(),
            EventDate = dtpEventDate.Value,
            IsActive = chkIsActive.Checked
        };

        if (dgvEvents.SelectedRows.Count > 0)
        {
            item.Id = Convert.ToInt32(dgvEvents.SelectedRows[0].Cells[0].Value);
        }

        DatabaseHelper.SaveEvent(item);
        LoadEvents();
        ClearForm();
        MessageBox.Show("Event saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearForm()
    {
        txtName.Clear();
        txtDescription.Clear();
        txtLocation.Clear();
        dtpEventDate.Value = DateTime.Now;
        chkIsActive.Checked = true;
        dgvEvents.ClearSelection();
    }
}
