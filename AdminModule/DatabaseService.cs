using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.AdminModule;

public class DatabaseService
{
    private const string ConnectionString = "User Id=sys;Password=1234567890;Data Source=localhost:1521/xe;DBA Privilege=SYSDBA;";

    private OracleConnection GetConnection()
    {
        OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        
        using (var cmd = new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\" = true", conn))
        {
            cmd.ExecuteNonQuery();
        }
        
        return conn;
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

    // --- User & Role Management with Search ---

    public DataTable GetAllUsers(string keyword = "")
    {
        string filter = string.IsNullOrEmpty(keyword) ? "" : "WHERE UPPER(USERNAME) LIKE :keyword";
        string query = $@"SELECT USERNAME, 
                               ACCOUNT_STATUS AS STATUS, 
                               CREATED, 
                               DECODE(ORACLE_MAINTAINED, 'Y', 'System', 'User') AS TYPE 
                        FROM DBA_USERS 
                        {filter} 
                        ORDER BY TYPE DESC, USERNAME ASC";
        
        OracleParameter[]? p = string.IsNullOrEmpty(keyword) ? null : new[] { new OracleParameter("keyword", $"%{keyword.ToUpper()}%") };
        return ExecuteQuery(query, p);
    }

    public DataTable GetAllRoles(string keyword = "")
    {
        string filter = string.IsNullOrEmpty(keyword) ? "" : "WHERE UPPER(ROLE) LIKE :keyword";
        string query = $@"SELECT ROLE, 
                               ROLE_ID, 
                               DECODE(ORACLE_MAINTAINED, 'Y', 'System', 'User') AS TYPE 
                        FROM DBA_ROLES 
                        {filter} 
                        ORDER BY TYPE DESC, ROLE ASC";
        
        OracleParameter[]? p = string.IsNullOrEmpty(keyword) ? null : new[] { new OracleParameter("keyword", $"%{keyword.ToUpper()}%") };
        return ExecuteQuery(query, p);
    }

    public void CreateUser(string username, string password) => ExecuteNonQuery($"CREATE USER {username} IDENTIFIED BY \"{password}\"");
    public void DropUser(string username) => ExecuteNonQuery($"DROP USER {username} CASCADE");
    public void AlterUserPassword(string username, string newPassword) => ExecuteNonQuery($"ALTER USER {username} IDENTIFIED BY \"{newPassword}\"");
    public void SetUserLockStatus(string username, bool lockUser) => ExecuteNonQuery($"ALTER USER {username} ACCOUNT {(lockUser ? "LOCK" : "UNLOCK")}");
    public void CreateRole(string roleName) => ExecuteNonQuery($"CREATE ROLE {roleName}");
    public void DropRole(string roleName) => ExecuteNonQuery($"DROP ROLE {roleName}");

    public DataTable GetUserRoles(string username)
    {
        return ExecuteQuery(
            @"SELECT GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE 
              FROM DBA_ROLE_PRIVS 
              WHERE GRANTEE = UPPER(:name) 
              ORDER BY GRANTED_ROLE",
            new[] { new OracleParameter("name", username) });
    }

    // --- Privilege Management ---

    public DataTable GetObjects(string objectType, bool hideSystem = true)
    {
        if (objectType == "ROLE")
        {
            string filter = hideSystem ? "WHERE ORACLE_MAINTAINED = 'N'" : "";
            return ExecuteQuery($@"SELECT ROLE AS OBJECT_NAME, 'ROLE' AS OWNER, DECODE(ORACLE_MAINTAINED, 'Y', 'System', 'User') AS TYPE
                                 FROM DBA_ROLES 
                                 {filter}
                                 ORDER BY TYPE DESC, ROLE");
        }

        if (objectType == "SYSTEM PRIVILEGE")
        {
            return ExecuteQuery(@"SELECT NAME AS OBJECT_NAME, 'SYSTEM' AS OWNER, 'System' AS TYPE 
                                 FROM SYSTEM_PRIVILEGE_MAP 
                                 ORDER BY NAME");
        }

        string objFilter = hideSystem ? "AND ORACLE_MAINTAINED = 'N'" : "";
        OracleParameter[] parameters = { new OracleParameter("objType", objectType) };
        
        return ExecuteQuery($@"SELECT (OWNER || '.' || OBJECT_NAME) AS OBJECT_NAME, OWNER, DECODE(ORACLE_MAINTAINED, 'Y', 'System', 'User') AS TYPE
                             FROM ALL_OBJECTS 
                             WHERE OBJECT_TYPE = :objType 
                             {objFilter}
                             AND OWNER NOT IN ('XDB', 'OUTLN', 'MDSYS')
                             ORDER BY TYPE DESC, OBJECT_NAME", parameters);
    }

    public DataTable GetTableColumns(string fullTableName)
    {
        string owner = "";
        string table = fullTableName;
        if (fullTableName.Contains('.'))
        {
            var parts = fullTableName.Split('.');
            owner = parts[0];
            table = parts[1];
        }

        string query = "SELECT COLUMN_NAME FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = UPPER(:tableName)";
        List<OracleParameter> p = new List<OracleParameter> { new OracleParameter("tableName", table) };
        if (!string.IsNullOrEmpty(owner))
        {
            query += " AND OWNER = UPPER(:owner)";
            p.Add(new OracleParameter("owner", owner));
        }
        query += " ORDER BY COLUMN_ID";

        return ExecuteQuery(query, p.ToArray());
    }

    public void GrantPrivilege(string grantee, string privilege, string? objectName, bool withGrantOption, List<string>? columns = null)
    {
        string sql = "";
        bool isObjectPriv = !string.IsNullOrEmpty(objectName);
        string grantOpt = withGrantOption ? (isObjectPriv ? " WITH GRANT OPTION" : " WITH ADMIN OPTION") : "";
        
        if (!isObjectPriv)
        {
            // System privilege or Role — no ON clause
            sql = $"GRANT {privilege} TO {grantee}{grantOpt}";
        }
        else
        {
            // Object privilege — requires ON clause
            bool hasColumns = columns != null && columns.Count > 0;
            // Only UPDATE supports column-level grants in Oracle XE. SELECT does NOT.
            bool supportsColumnLevel = privilege.ToUpper() == "UPDATE";
            
            if (hasColumns && supportsColumnLevel)
            {
                string cols = string.Join(", ", columns!);
                sql = $"GRANT {privilege} ({cols}) ON {objectName} TO {grantee}{grantOpt}";
            }
            else
            {
                sql = $"GRANT {privilege} ON {objectName} TO {grantee}{grantOpt}";
            }
        }
        
        try
        {
            ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            throw new Exception($"SQL thực thi:\n{sql}\n\nLỗi Oracle:\n{ex.Message}", ex);
        }
    }

    private bool IsObjectPrivilege(string priv)
    {
        string p = priv.ToUpper();
        return p == "SELECT" || p == "INSERT" || p == "UPDATE" || p == "DELETE" || p == "EXECUTE" || p == "ALL";
    }

    public bool IsRole(string name)
    {
        DataTable dt = ExecuteQuery("SELECT 1 FROM DBA_ROLES WHERE ROLE = UPPER(:name)", new[] { new OracleParameter("name", name) });
        return dt.Rows.Count > 0;
    }

    public bool IsSystemPrivilege(string name)
    {
        DataTable dt = ExecuteQuery("SELECT 1 FROM SYSTEM_PRIVILEGE_MAP WHERE NAME = UPPER(:name)", new[] { new OracleParameter("name", name) });
        return dt.Rows.Count > 0;
    }

    public void RevokePrivilege(string grantee, string privilege, string? objectName)
    {
        string sql;
        bool isObjectPriv = !string.IsNullOrEmpty(objectName);

        if (!isObjectPriv)
        {
            // System privilege or Role
            sql = $"REVOKE {privilege} FROM {grantee}";
        }
        else
        {
            // Object privilege
            // Note: Oracle doesn't support revoking specific columns, you revoke the privilege on the table.
            // If privilege contains column info like UPDATE(COL1), we strip it.
            string cleanPriv = privilege;
            if (privilege.Contains('('))
            {
                cleanPriv = privilege.Substring(0, privilege.IndexOf('('));
            }
            sql = $"REVOKE {cleanPriv} ON {objectName} FROM {grantee}";
        }
        ExecuteNonQuery(sql);
    }

    public DataTable GetUserOrRolePrivileges(string name)
    {
        string sql = @"
            WITH role_hierarchy AS (
                SELECT GRANTED_ROLE FROM DBA_ROLE_PRIVS 
                START WITH GRANTEE = UPPER(:name) 
                CONNECT BY PRIOR GRANTED_ROLE = GRANTEE
            ),
            all_grantees AS (
                SELECT UPPER(:name) AS GRANTEE FROM DUAL
                UNION
                SELECT GRANTED_ROLE FROM role_hierarchy
            )
            SELECT DISTINCT 'System' AS TYPE, PRIVILEGE AS PRIVILEGE, NULL AS OBJECT_NAME, ADMIN_OPTION AS WITH_GRANT 
            FROM DBA_SYS_PRIVS WHERE GRANTEE IN (SELECT GRANTEE FROM all_grantees)
            UNION ALL
            SELECT DISTINCT 'Role' AS TYPE, GRANTED_ROLE AS PRIVILEGE, NULL AS OBJECT_NAME, ADMIN_OPTION AS WITH_GRANT 
            FROM DBA_ROLE_PRIVS WHERE GRANTEE IN (SELECT GRANTEE FROM all_grantees)
            UNION ALL
            SELECT DISTINCT 'Object' AS TYPE, PRIVILEGE AS PRIVILEGE, OWNER || '.' || TABLE_NAME AS OBJECT_NAME, GRANTABLE AS WITH_GRANT 
            FROM DBA_TAB_PRIVS WHERE GRANTEE IN (SELECT GRANTEE FROM all_grantees)
            UNION ALL
            SELECT DISTINCT 'Column' AS TYPE, PRIVILEGE || '(' || COLUMN_NAME || ')' AS PRIVILEGE, OWNER || '.' || TABLE_NAME AS OBJECT_NAME, GRANTABLE AS WITH_GRANT 
            FROM DBA_COL_PRIVS WHERE GRANTEE IN (SELECT GRANTEE FROM all_grantees)";
        
        return ExecuteQuery(sql, new[] { new OracleParameter("name", name) });
    }
}
