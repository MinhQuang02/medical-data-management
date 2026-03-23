using System;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace MedicalDataManagement.AdminModule;

public class DatabaseSeeder
{
    private static readonly string ConnectionString = "User Id=sys;Password=1234567890;Data Source=localhost:1521/XE;DBA Privilege=SYSDBA;";

    public static string InitializeDatabase(string scriptsFolderPath)
    {
        try
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // 1. Setup hệ thống để tạo user trên XE không cần C##
                ExecuteNonQuery(conn, "ALTER SESSION SET \"_ORACLE_SCRIPT\"=true");

                // 2. Xóa user QLBENHVIEN nếu đã tồn tại để làm sạch
                ExecutePlSql(conn, "BEGIN \n EXECUTE IMMEDIATE 'DROP USER QLBENHVIEN CASCADE'; \n EXCEPTION WHEN OTHERS THEN NULL; \n END;");

                // 3. Tạo Schema QLBENHVIEN và cấp quyền cơ bản
                string[] initCmds = new string[] {
                    "CREATE USER QLBENHVIEN IDENTIFIED BY 123",
                    "GRANT CREATE SESSION TO QLBENHVIEN",
                    "GRANT CREATE TABLE TO QLBENHVIEN",
                    "GRANT CREATE VIEW TO QLBENHVIEN",
                    "ALTER USER QLBENHVIEN QUOTA UNLIMITED ON USERS",
                    "GRANT CREATE USER TO QLBENHVIEN",
                    "GRANT EXECUTE ON DBMS_RLS TO QLBENHVIEN",
                    "GRANT CREATE PROCEDURE TO QLBENHVIEN",
                    "GRANT CREATE ROLE TO QLBENHVIEN",
                    "GRANT DROP USER TO QLBENHVIEN",
                    "GRANT ALTER USER TO QLBENHVIEN",
                    "GRANT CREATE ANY CONTEXT TO QLBENHVIEN",
                    "GRANT AUDIT SYSTEM TO QLBENHVIEN",
                    "GRANT AUDIT ANY TO QLBENHVIEN"
                };

                foreach (var cmdText in initCmds)
                {
                    try {
                        ExecuteNonQuery(conn, cmdText);
                    } catch (Exception ex) {
                        return $"[Lỗi lệnh hệ thống: {cmdText}]\n{ex.Message}";
                    }
                }

                // 4. Chuyển Session sang Schema QLBENHVIEN để các bảng tạo ra nằm đúng schema
                ExecuteNonQuery(conn, "ALTER SESSION SET CURRENT_SCHEMA = QLBENHVIEN");

                // 5. Chạy file table_storedproc.sql
                string tableScriptPath = Path.Combine(scriptsFolderPath, "table_storedproc.sql");
                if (File.Exists(tableScriptPath))
                {
                    try {
                        RunSqlScript(conn, tableScriptPath);
                    } catch (Exception ex) {
                        return $"[Lỗi trong file table_storedproc.sql]\n{ex.Message}";
                    }
                }
                else
                {
                    return "[Lỗi] Không tìm thấy file table_storedproc.sql";
                }

                // 6. Chạy file user_role_view_func.sql
                string roleScriptPath = Path.Combine(scriptsFolderPath, "user_role_view_func.sql");
                if (File.Exists(roleScriptPath))
                {
                    try {
                        RunSqlScript(conn, roleScriptPath);
                    } catch (Exception ex) {
                        return $"[Lỗi trong file user_role_view_func.sql]\n{ex.Message}";
                    }
                }
                else
                {
                    return "[Lỗi] Không tìm thấy file user_role_view_func.sql";
                }

                return "[Thành công] Đã khởi tạo và đẩy toàn bộ script lên Oracle Database thành công!";
            }
        }
        catch (Exception ex)
        {
            return $"[Lỗi khởi tạo DB]\n{ex.Message}";
        }
    }

    private static void ExecuteNonQuery(OracleConnection conn, string sql)
    {
        using (OracleCommand cmd = new OracleCommand(sql, conn))
        {
            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception($"[SQL Error: {sql}] - {ex.Message}", ex);
            }
        }
    }

    private static void ExecutePlSql(OracleConnection conn, string sql)
    {
        using (OracleCommand cmd = new OracleCommand(sql, conn))
        {
            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception($"[PLSQL Error: {sql}] - {ex.Message}", ex);
            }
        }
    }

    public static void RunSqlScript(OracleConnection conn, string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        string currentDbCommand = "";
        bool isInPlSql = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            // Bỏ qua comment
            if (line.StartsWith("--") || string.IsNullOrEmpty(line)) continue;

            // Xử lý báo hiệu của câu lệnh PL/SQL
            if (line.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase) || 
                line.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE OR REPLACE PROCEDURE", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE OR REPLACE FUNCTION", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE OR REPLACE TRIGGER", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE OR REPLACE TYPE", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE PROCEDURE", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CREATE FUNCTION", StringComparison.OrdinalIgnoreCase))
            {
                isInPlSql = true;
            }

            // Dấu '/' đơn trên 1 dòng báo hiệu kết thúc và chạy khối lệnh PL/SQL
            if (line == "/" && isInPlSql)
            {
                if (!string.IsNullOrWhiteSpace(currentDbCommand))
                {
                    ExecutePlSql(conn, currentDbCommand);
                    currentDbCommand = "";
                }
                isInPlSql = false;
                continue;
            }

            // Nếu không phải trong khối lệnh PL/SQL và gặp dấu chấm phẩy, coi đó là 1 command DDL/DML thông thường
            if (!isInPlSql && line.EndsWith(";"))
            {
                currentDbCommand += " " + line.Substring(0, line.Length - 1); // Loại bỏ dấu ; (OracleCommand không hỗ trợ ; ở cuối với DDL/DML thuần)
                if (!string.IsNullOrWhiteSpace(currentDbCommand))
                {
                    ExecuteNonQuery(conn, currentDbCommand);
                    currentDbCommand = "";
                }
                continue;
            }

            currentDbCommand += " " + line + "\n";
        }

        // Chạy rớt lại nếu có
        if (!string.IsNullOrWhiteSpace(currentDbCommand))
        {
            ExecuteNonQuery(conn, currentDbCommand);
        }
    }
}
