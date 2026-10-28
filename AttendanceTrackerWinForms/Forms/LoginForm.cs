using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class UserRoleForm : Form
{
    private readonly DataGridView dgvUsers = new();
    private readonly ComboBox cmbRole = new();
    private readonly Button btnUpdate = new();

    public UserRoleForm()
    {
        Text = "User Role Management";
        Width = 700;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        dgvUsers.Location = new Point(20, 20);
        dgvUsers.Size = new Size(440, 360);
        dgvUsers.ReadOnly = true;
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvUsers.CellClick += (_, _) => LoadSelectedRole();

        var lblRole = new Label { Text = "Role", Location = new Point(480, 30), Size = new Size(60, 20) };
        cmbRole.Location = new Point(480, 55);
        cmbRole.Size = new Size(170, 28);
        cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRole.Items.AddRange(new object[] { "Admin", "Manager", "Employee" });

        btnUpdate.Text = "Update Role";
        btnUpdate.Location = new Point(480, 95);
        btnUpdate.Size = new Size(140, 34);
        btnUpdate.Click += (_, _) => UpdateSelectedRole();

        Controls.Add(dgvUsers);
        Controls.Add(lblRole);
        Controls.Add(cmbRole);
        Controls.Add(btnUpdate);

        LoadUsers();
    }

    private void LoadUsers()
    {
        dgvUsers.DataSource = DatabaseHelper.GetUsers();
    }

    private void LoadSelectedRole()
    {
        if (dgvUsers.SelectedRows.Count == 0)
        {
            return;
        }

        var row = dgvUsers.SelectedRows[0];
        var role = row.Cells[3].Value?.ToString() ?? "Employee";
        cmbRole.SelectedItem = role;
    }

    private void UpdateSelectedRole()
    {
        if (dgvUsers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a user.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells[0].Value);
        var role = cmbRole.SelectedItem?.ToString() ?? "Employee";

        DatabaseHelper.UpdateUserRole(userId, role);
        LoadUsers();
        MessageBox.Show("User role updated successfully.", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
