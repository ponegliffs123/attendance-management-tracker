using System.Windows.Forms;

namespace AttendanceTrackerWinForms.Forms;

public class LoginForm : Form
{
    private readonly TextBox txtUsername = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnLogin = new();
    private readonly Label lblUsername = new();
    private readonly Label lblPassword = new();
    private readonly Label lblTitle = new();

    public LoginForm()
    {
        Text = "Attendance Tracker - Login";
        Width = 420;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;

        lblTitle.Text = "Attendance Management";
        lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        lblTitle.Location = new Point(110, 20);
        lblTitle.Size = new Size(220, 35);

        lblUsername.Text = "Username";
        lblUsername.Location = new Point(50, 80);
        lblUsername.Size = new Size(80, 20);

        txtUsername.Location = new Point(140, 76);
        txtUsername.Size = new Size(200, 28);
        txtUsername.Text = "admin";

        lblPassword.Text = "Password";
        lblPassword.Location = new Point(50, 120);
        lblPassword.Size = new Size(80, 20);

        txtPassword.Location = new Point(140, 116);
        txtPassword.Size = new Size(200, 28);
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.Text = "admin123";

        btnLogin.Text = "Login";
        btnLogin.Location = new Point(140, 170);
        btnLogin.Size = new Size(200, 34);
        btnLogin.Click += BtnLogin_Click;

        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(lblPassword);
        Controls.Add(txtUsername);
        Controls.Add(txtPassword);
        Controls.Add(btnLogin);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var user = DatabaseHelper.ValidateLogin(txtUsername.Text, txtPassword.Text);
        if (user == null)
        {
            MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var mainForm = new MainForm(user);
        mainForm.Show();
        Hide();
    }
}
