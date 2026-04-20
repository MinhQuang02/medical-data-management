using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;

namespace MedicalDataManagement.AdminModule;

/// <summary>
/// Helper class for Oracle database operations.
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Oracle connection string for the local XE instance.
    /// </summary>
    private const string ConnectionString = "User Id=sys;Password=1234567890;Data Source=localhost:1521/xe;DBA Privilege=SYSDBA;";

    /// <summary>
    /// Verifies that the Oracle database is reachable and operational.
    /// If connection fails, displays an error message to the user.
    /// </summary>
    /// <returns>True if connection is successful; otherwise, false.</returns>
    public static bool CheckConnection()
    {
        try
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                // Attempt to open the connection to verify availability
                connection.Open();
                
                // Connection successful, no message needed
                return true;
            }
        }
        catch (OracleException ex)
        {
            // Specifically handling Oracle-related connection errors
            DisplayConnectionError(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            // Handling any other potential exceptions during connection attempt
            DisplayConnectionError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Displays a standardized error message to the user when the database is unreachable.
    /// </summary>
    /// <param name="errorMessage">Detailed error message from the exception.</param>
    private static void DisplayConnectionError(string errorMessage)
    {
        MessageBox.Show(
            "Cannot connect to the database. Please check if the Oracle service is running.\n\n" +
            "Details: " + errorMessage,
            "Database Connection Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
