namespace MedicalDataManagement.AdminModule;



static class Program
{
    [STAThread]
    static void Main()
    {
        // First verify that the Oracle database is reachable and operational
        if (!DatabaseHelper.CheckConnection())
        {
            // If connection fails, message is already shown by CheckConnection()
            // Terminate the application safely.
            Environment.Exit(1);
        }

        // Database connection successful, proceed with application launch
        ApplicationConfiguration.Initialize();
        Application.Run(new AdminDashboard());
    }
}
