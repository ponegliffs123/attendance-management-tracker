using AttendanceTrackerWinForms.Forms;

namespace AttendanceTrackerWinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseHelper.EnsureDatabase();
        Application.Run(new LoginForm());
    }
}
