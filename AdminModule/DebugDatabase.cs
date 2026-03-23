using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MedicalDataManagement.AdminModule;

public static class DebugDatabase
{
    private static readonly string ConnectionString = "User Id=sys;Password=1234567890;Data Source=localhost:1521/XE;DBA Privilege=SYSDBA;";

    public static string CheckHSBA_DVColumns()
    {
        try
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("SELECT column_name FROM all_tab_columns WHERE owner = 'QLBENHVIEN' AND table_name = 'HSBA_DV'", conn))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        string result = "Columns in QLBENHVIEN.HSBA_DV:\n";
                        while (reader.Read())
                        {
                            result += reader.GetString(0) + "\n";
                        }
                        return result;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return "Error checking columns: " + ex.Message;
        }
    }
}
