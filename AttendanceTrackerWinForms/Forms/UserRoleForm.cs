using System.Windows.Forms;
using QRCoder;
using ZXing;

namespace AttendanceTrackerWinForms.Forms;

public class AttendanceRecordForm : Form
{
    private readonly ComboBox cmbEvent = new();
    private readonly ComboBox cmbAttendee = new();
    private readonly DateTimePicker dtpCheckIn = new();
    private readonly TextBox txtNotes = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();
    private readonly Button btnGenerateQr = new();
    private readonly Button btnScanQr = new();
    private readonly PictureBox pbQrImage = new();
    private readonly DataGridView dgvAttendance = new();

    public AttendanceRecordForm()
    {
        Text = "Attendance Recording";
        Width = 980;
        Height = 620;
        StartPosition = FormStartPosition.CenterParent;

        var labelEvent = new Label { Text = "Event", Location = new Point(20, 20), Size = new Size(100, 20) };
        cmbEvent.Location = new Point(130, 18);
        cmbEvent.Size = new Size(250, 28);
        cmbEvent.DropDownStyle = ComboBoxStyle.DropDownList;

        var labelAttendee = new Label { Text = "Attendee", Location = new Point(20, 60), Size = new Size(100, 20) };
        cmbAttendee.Location = new Point(130, 58);
        cmbAttendee.Size = new Size(250, 28);
        cmbAttendee.DropDownStyle = ComboBoxStyle.DropDownList;

        var labelCheckIn = new Label { Text = "Check-in Time", Location = new Point(20, 100), Size = new Size(100, 20) };
        dtpCheckIn.Location = new Point(130, 98);
        dtpCheckIn.Size = new Size(250, 28);
        dtpCheckIn.Format = DateTimePickerFormat.Custom;
        dtpCheckIn.CustomFormat = "dd/MM/yyyy HH:mm";

        var labelNotes = new Label { Text = "Notes", Location = new Point(20, 140), Size = new Size(100, 20) };
        txtNotes.Location = new Point(130, 138);
        txtNotes.Size = new Size(250, 80);
        txtNotes.Multiline = true;

        btnSave.Text = "Save Attendance";
        btnSave.Location = new Point(130, 240);
        btnSave.Size = new Size(150, 34);
        btnSave.Click += (_, _) => SaveAttendance();

        btnGenerateQr.Text = "Generate QR";
        btnGenerateQr.Location = new Point(130, 285);
        btnGenerateQr.Size = new Size(150, 34);
        btnGenerateQr.Click += (_, _) => GenerateQrForSelectedEntry();

        btnScanQr.Text = "Scan QR";
        btnScanQr.Location = new Point(300, 285);
        btnScanQr.Size = new Size(100, 34);
        btnScanQr.Click += (_, _) => ScanQrFromFile();

        btnCancel.Text = "Close";
        btnCancel.Location = new Point(300, 240);
        btnCancel.Size = new Size(80, 34);
        btnCancel.Click += (_, _) => Close();

        pbQrImage.Location = new Point(130, 330);
        pbQrImage.Size = new Size(220, 180);
        pbQrImage.SizeMode = PictureBoxSizeMode.StretchImage;
        pbQrImage.BorderStyle = BorderStyle.FixedSingle;

        dgvAttendance.Location = new Point(430, 20);
        dgvAttendance.Size = new Size(500, 500);
        dgvAttendance.ReadOnly = true;
        dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(labelEvent);
        Controls.Add(cmbEvent);
        Controls.Add(labelAttendee);
        Controls.Add(cmbAttendee);
        Controls.Add(labelCheckIn);
        Controls.Add(dtpCheckIn);
        Controls.Add(labelNotes);
        Controls.Add(txtNotes);
        Controls.Add(btnSave);
        Controls.Add(btnGenerateQr);
        Controls.Add(btnScanQr);
        Controls.Add(btnCancel);
        Controls.Add(pbQrImage);
        Controls.Add(dgvAttendance);

        LoadCombos();
        LoadAttendance();
    }

    private void LoadCombos()
    {
        var eventTable = DatabaseHelper.GetEventsForComboBox();
        cmbEvent.DataSource = eventTable;
        cmbEvent.DisplayMember = "Name";
        cmbEvent.ValueMember = "Id";

        var attendeeTable = DatabaseHelper.GetAttendeesForComboBox();
        cmbAttendee.DataSource = attendeeTable;
        cmbAttendee.DisplayMember = "DisplayName";
        cmbAttendee.ValueMember = "Id";
    }

    private void LoadAttendance()
    {
        dgvAttendance.DataSource = DatabaseHelper.GetAttendanceRecords();
    }

    private void GenerateQrForSelectedEntry()
    {
        if (cmbEvent.SelectedValue == null || cmbAttendee.SelectedValue == null)
        {
            MessageBox.Show("Please select an event and attendee.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var eventId = Convert.ToInt32(cmbEvent.SelectedValue);
        var attendeeId = Convert.ToInt32(cmbAttendee.SelectedValue);
        var payload = $"event:{eventId}|attendee:{attendeeId}";

        var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var code = new PngByteQRCode(data);
        var bytes = code.GetGraphic(20);

        using var ms = new MemoryStream(bytes);
        pbQrImage.Image = Image.FromStream(ms);
    }

    private void ScanQrFromFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Select QR code image"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var bitmap = new Bitmap(dialog.FileName);
            var reader = new BarcodeReader();
            var result = reader.Decode(bitmap);

            if (result == null)
            {
                MessageBox.Show("No QR code found in the selected image.", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var payload = result.Text.Trim();
            if (!payload.Contains("event:") || !payload.Contains("|attendee:"))
            {
                MessageBox.Show("The scanned QR code is not a valid attendance code.", "Invalid QR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var eventId = int.Parse(parts[0].Substring("event:".Length));
            var attendeeId = int.Parse(parts[1].Substring("attendee:".Length));

            cmbEvent.SelectedValue = eventId;
            cmbAttendee.SelectedValue = attendeeId;
            MessageBox.Show("Attendance QR scanned successfully.", "QR Scan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to scan QR code: {ex.Message}", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveAttendance()
    {
        if (cmbEvent.SelectedValue == null || cmbAttendee.SelectedValue == null)
        {
            MessageBox.Show("Please select an event and attendee.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var record = new AttendanceRecord
        {
            EventId = Convert.ToInt32(cmbEvent.SelectedValue),
            AttendeeId = Convert.ToInt32(cmbAttendee.SelectedValue),
            CheckInTime = dtpCheckIn.Value,
            Notes = txtNotes.Text.Trim()
        };

        var saved = DatabaseHelper.SaveAttendanceRecord(record);
        if (!saved)
        {
            MessageBox.Show("This attendee was already marked present for the selected event on that date.", "Duplicate Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LoadAttendance();
        txtNotes.Clear();
        MessageBox.Show("Attendance saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
