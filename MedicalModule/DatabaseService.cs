using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.MedicalModule;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string username, string password)
    {
        _connectionString = $"User Id={username};Password={password};Data Source=localhost:1521/xe;";
    }

    private OracleConnection GetConnection()
    {
        OracleConnection conn = new OracleConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public bool TestConnection()
    {
        try
        {
            using (var conn = GetConnection())
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public DataTable ExecuteQuery(string query, OracleParameter[]? parameters = null)
    {
        using (var conn = GetConnection())
        {
            using (var cmd = new OracleCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }

    public void ExecuteNonQuery(string query, OracleParameter[]? parameters = null)
    {
        using (var conn = GetConnection())
        {
            using (var cmd = new OracleCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public List<string> GetCurrentUserRoles()
    {
        List<string> roles = new List<string>();
        string query = "SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS";
        DataTable dt = ExecuteQuery(query);
        foreach(DataRow row in dt.Rows)
        {
            roles.Add(row["ROLE"].ToString()!.ToUpper());
        }
        return roles;
    }
}
