using System.Data;
using MedicalDataManagement.Common;
using Oracle.ManagedDataAccess.Client;

namespace MedicalDataManagement.AdminModule;

public partial class AdminDashboard : Form
{
    private readonly DatabaseService _dbService;

    public AdminDashboard()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        
        ConfigureGrid(dgvUsers);
        ConfigureGrid(dgvRoles);
        ConfigureGrid(dgvPrivileges);
        ConfigureGrid(dgvUserCurrentRoles);
        dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvRoles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvPrivileges.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        this.Load += AdminDashboard_Load;
        SetNavActive(btnNavUsers);
        
        // --- Search Events ---
        txtQuickSearchUR.TextChanged += (s, e) => LoadUsersAndRoles();
        chkOnlySystem_Priv.CheckedChanged += (s, e) => { LoadObjects(); LoadGrantees(); };

        // --- CRUD Handlers ---
        btnCreateUser.Click += btnCreateUser_Click;
        btnEditUser.Click += btnEditUser_Click;
        btnDropUser.Click += btnDropUser_Click;
        btnCreateRole.Click += btnCreateRole_Click;
        btnDropRole.Click += btnDropRole_Click;
        btnGrant.Click += btnGrant_Click;
        btnRevoke.Click += btnRevoke_Click;
        btnSearchPrivileges.Click += btnSearchPrivileges_Click;
        
        // NOTE: cbObjectType has ONE handler for loading objects + updating privilege types
        cbObjectName.SelectedIndexChanged += cbObjectName_SelectedIndexChanged;
        cbPrivilegeType.SelectedIndexChanged += (s, e) => UpdateColumnListState();

        // ComboBox setup - ObjectType drives all other combos
        cbObjectType.Items.Clear();
        cbObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION", "ROLE", "SYSTEM PRIVILEGE" });
        cbObjectType.SelectedIndexChanged += (s, ev) => {
             cbPrivilegeType.Items.Clear();
             string type = cbObjectType.SelectedItem?.ToString() ?? "TABLE";
             if (type == "TABLE" || type == "VIEW") cbPrivilegeType.Items.AddRange(new object[] { "SELECT", "INSERT", "UPDATE", "DELETE", "ALL" });
             else if (type == "ROLE" || type == "SYSTEM PRIVILEGE") cbPrivilegeType.Items.AddRange(new object[] { "GRANT" });
             else cbPrivilegeType.Items.AddRange(new object[] { "EXECUTE" });
             if (cbPrivilegeType.Items.Count > 0) cbPrivilegeType.SelectedIndex = 0;
             cbPrivilegeType.Enabled = (type != "ROLE" && type != "SYSTEM PRIVILEGE");
             LoadObjects();
        };
        if (cbObjectType.Items.Count > 0) cbObjectType.SelectedIndex = 0;

        // --- Assign Role Panel ---
        btnAssignRole.Click += btnAssignRole_Click;
        btnRevokeRole.Click += btnRevokeRole_Click;
        cbRoleUser.SelectedIndexChanged += (s, e) => LoadUserCurrentRoles();
        
        // Context menu for User grid
        ContextMenuStrip userMenu = new ContextMenuStrip();
        userMenu.Items.Add("Khóa tài khoản", null, (s, e) => ToggleUserLock(true));
        userMenu.Items.Add("Mở khóa tài khoản", null, (s, e) => ToggleUserLock(false));
        dgvUsers.ContextMenuStrip = userMenu;
    }

    private void UpdateColumnListState()
    {
        string priv = cbPrivilegeType.Text;
        // Oracle only supports column-level grants for UPDATE (and REFERENCES).
        // SELECT is NOT column-level grantable in Oracle XE — it's TABLE-level only.
        bool allowed = priv == "UPDATE";
        clbColumns.Enabled = allowed;
        if (!allowed) { for (int i = 0; i < clbColumns.Items.Count; i++) clbColumns.SetItemChecked(i, false); }
    }

    private void ConfigureGrid(DataGridView dgv)
    {
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.RowHeadersVisible = false;
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.None;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(244, 246, 249);
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Bold", 9);
        dgv.ColumnHeadersHeight = 40;
        dgv.GridColor = Color.FromArgb(238, 241, 245);
    }

    private void AdminDashboard_Load(object? sender, EventArgs e)
    {
        LoadUsersAndRoles();
        LoadObjects();
        LoadGrantees();
        LoadRoleAssignPanel();
    }

    private void LoadUsersAndRoles()
    {
        try
        {
            string keyword = txtQuickSearchUR.Text.Trim();
            dgvUsers.DataSource = _dbService.GetAllUsers(keyword);
            dgvRoles.DataSource = _dbService.GetAllRoles(keyword);
        }
        catch (Exception ex) { ShowError("Lỗi danh sách: " + ex.Message); }
    }

    private void LoadGrantees()
    {
        try
        {
            DataTable users = _dbService.GetAllUsers("");
            DataTable roles = _dbService.GetAllRoles("");
            cbGrantee.Items.Clear();
            foreach (DataRow row in users.Rows) cbGrantee.Items.Add(row["USERNAME"].ToString()!);
            foreach (DataRow row in roles.Rows) cbGrantee.Items.Add(row["ROLE"].ToString()!);
            if (cbGrantee.Items.Count > 0) cbGrantee.SelectedIndex = 0;
        }
        catch (Exception ex) { ShowError("Lỗi Grantee: " + ex.Message); }
    }

    private void LoadRoleAssignPanel()
    {
        try
        {
            DataTable users = _dbService.GetAllUsers("");
            DataTable roles = _dbService.GetAllRoles("");
            cbRoleUser.Items.Clear();
            cbRoleName.Items.Clear();
            foreach (DataRow row in users.Rows) cbRoleUser.Items.Add(row["USERNAME"].ToString()!);
            foreach (DataRow row in roles.Rows) cbRoleName.Items.Add(row["ROLE"].ToString()!);
            if (cbRoleUser.Items.Count > 0) cbRoleUser.SelectedIndex = 0;
            if (cbRoleName.Items.Count > 0) cbRoleName.SelectedIndex = 0;
        }
        catch (Exception ex) { ShowError("Lỗi load panel role: " + ex.Message); }
    }

    private void LoadUserCurrentRoles()
    {
        try
        {
            string user = cbRoleUser.SelectedItem?.ToString() ?? "";
            if (!string.IsNullOrEmpty(user))
                dgvUserCurrentRoles.DataSource = _dbService.GetUserRoles(user);
        }
        catch (Exception ex) { ShowError("Lỗi tải role của user: " + ex.Message); }
    }

    private void LoadObjects()
    {
        try
        {
            string type = cbObjectType.SelectedItem?.ToString() ?? "TABLE";
            bool hideSystem = chkOnlySystem_Priv.Checked;
            DataTable objects = _dbService.GetObjects(type, hideSystem);
            cbObjectName.DataSource = objects;
            cbObjectName.DisplayMember = "OBJECT_NAME";
        }
        catch (Exception ex) { ShowError("Lỗi đối tượng: " + ex.Message); }
    }

    // cbObjectType now has only ONE SelectedIndexChanged handler (the lambda in constructor).
    // This method is kept for compatibility but the lambda handles both privilege list update AND LoadObjects().
    private void cbObjectType_SelectedIndexChanged(object? sender, EventArgs e) { /* handled in constructor lambda */ }

    // Helper to safely get the selected object name from the data-bound ComboBox
    private string GetSelectedObjectName()
    {
        if (cbObjectName.SelectedItem is System.Data.DataRowView drv)
        {
            return drv["OBJECT_NAME"]?.ToString()?.Trim() ?? "";
        }
        return cbObjectName.Text.Trim();
    }

    private void cbObjectName_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            clbColumns.Items.Clear();
            if (cbObjectType.SelectedItem?.ToString() == "TABLE")
            {
                string tableName = cbObjectName.Text;
                if (!string.IsNullOrEmpty(tableName))
                {
                    DataTable cols = _dbService.GetTableColumns(tableName);
                    foreach (DataRow row in cols.Rows) clbColumns.Items.Add(row["COLUMN_NAME"].ToString()!);
                }
            }
            UpdateColumnListState();
        }
        catch (Exception ex) { ShowError("Lỗi cột: " + ex.Message); }
    }

    private void btnCreateUser_Click(object? sender, EventArgs e)
    {
        string username = Microsoft.VisualBasic.Interaction.InputBox("Tên user:", "Thêm User", "");
        string password = Microsoft.VisualBasic.Interaction.InputBox("Mật khẩu:", "Thêm User", "123");
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            try { _dbService.CreateUser(username, password); LoadUsersAndRoles(); LoadGrantees(); }
            catch (Exception ex) { ShowError(ex.Message); }
        }
    }

    private void btnEditUser_Click(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count > 0)
        {
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value.ToString()!;
            string pass = Microsoft.VisualBasic.Interaction.InputBox($"Mật khẩu mới cho {username}:", "Đổi mật khẩu", "");
            if (!string.IsNullOrEmpty(pass))
            {
                try { _dbService.AlterUserPassword(username, pass); MessageBox.Show("Đã đổi password."); }
                catch (Exception ex) { ShowError(ex.Message); }
            }
        }
    }

    private void btnDropUser_Click(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count > 0)
        {
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value.ToString()!;
            if (MessageBox.Show($"Xóa user {username}?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try { _dbService.DropUser(username); LoadUsersAndRoles(); LoadGrantees(); }
                catch (Exception ex) { ShowError(ex.Message); }
            }
        }
    }

    private void ToggleUserLock(bool lockIt)
    {
        if (dgvUsers.SelectedRows.Count > 0)
        {
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value.ToString()!;
            try {
                _dbService.SetUserLockStatus(username, lockIt);
                LoadUsersAndRoles();
                MessageBox.Show($"Đã {(lockIt ? "khóa" : "mở khóa")} user {username}");
            } catch (Exception ex) { ShowError(ex.Message); }
        }
    }

    private void btnCreateRole_Click(object? sender, EventArgs e)
    {
        string role = Microsoft.VisualBasic.Interaction.InputBox("Tên role:", "Thêm Role", "");
        if (!string.IsNullOrEmpty(role))
        {
            try { _dbService.CreateRole(role); LoadUsersAndRoles(); LoadGrantees(); }
            catch (Exception ex) { ShowError(ex.Message); }
        }
    }

    private void btnDropRole_Click(object? sender, EventArgs e)
    {
        if (dgvRoles.SelectedRows.Count > 0)
        {
            string role = dgvRoles.SelectedRows[0].Cells["ROLE"].Value.ToString()!;
            if (MessageBox.Show($"Xóa role {role}?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try { _dbService.DropRole(role); LoadUsersAndRoles(); LoadGrantees(); }
                catch (Exception ex) { ShowError(ex.Message); }
            }
        }
    }

    private void btnGrant_Click(object? sender, EventArgs e)
    {
        try
        {
            string grantee = cbGrantee.Text.Trim();
            string objType = cbObjectType.SelectedItem?.ToString()?.Trim() ?? "";
            string priv = cbPrivilegeType.Text.Trim();
            string obj = GetSelectedObjectName();
            bool withGrant = chkWithGrantOption.Checked;
            
            if (string.IsNullOrEmpty(grantee))
            {
                ShowError("Vui lòng chọn Grantee (người nhận quyền).");
                return;
            }
            if (string.IsNullOrEmpty(obj))
            {
                ShowError("Vui lòng chọn Đối tượng (Table, Role, Quyền hệ thống...).");
                return;
            }

            if (objType == "ROLE" || objType == "SYSTEM PRIVILEGE")
            {
                // obj is the role/sys-priv name, no ON clause needed
                _dbService.GrantPrivilege(grantee, obj, null, withGrant, null);
            }
            else
            {
                if (string.IsNullOrEmpty(priv))
                {
                    ShowError("Vui lòng chọn Loại quyền cụ thể (SELECT, INSERT...).");
                    return;
                }
                List<string> selectedCols = new List<string>();
                foreach (var item in clbColumns.CheckedItems) selectedCols.Add(item.ToString()!);
                _dbService.GrantPrivilege(grantee, priv, obj, withGrant, selectedCols);
            }
            MessageBox.Show($"Cấp quyền thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("Lỗi: " + ex.Message); }
    }

    private void btnRevoke_Click(object? sender, EventArgs e)
    {
        try {
            string grantee = cbGrantee.Text.Trim();
            string objType = cbObjectType.SelectedItem?.ToString()?.Trim() ?? "";
            string priv = cbPrivilegeType.Text.Trim();
            string obj = GetSelectedObjectName();

            if (string.IsNullOrEmpty(grantee))
            {
                ShowError("Vui lòng chọn Grantee (người cần thu hồi quyền).");
                return;
            }
            if (string.IsNullOrEmpty(obj))
            {
                ShowError("Vui lòng chọn Đối tượng.");
                return;
            }

            if (objType == "ROLE" || objType == "SYSTEM PRIVILEGE")
            {
                _dbService.RevokePrivilege(grantee, obj, null);
            }
            else
            {
                if (string.IsNullOrEmpty(priv))
                {
                    ShowError("Vui lòng chọn Loại quyền cụ thể.");
                    return;
                }
                _dbService.RevokePrivilege(grantee, priv, obj);
            }
            MessageBox.Show("Đã thu hồi quyền.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } catch (Exception ex) { ShowError(ex.Message); }
    }

    private void btnSearchPrivileges_Click(object? sender, EventArgs e)
    {
        try {
            string name = txtSearchUserRole.Text.Trim();
            if (!string.IsNullOrEmpty(name)) dgvPrivileges.DataSource = _dbService.GetUserOrRolePrivileges(name);
        } catch (Exception ex) { ShowError(ex.Message); }
    }

    private void btnAssignRole_Click(object? sender, EventArgs e)
    {
        try
        {
            string user = cbRoleUser.SelectedItem?.ToString() ?? "";
            string role = cbRoleName.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(role))
            {
                ShowError("Vui lòng chọn cả User và Role.");
                return;
            }
            bool withAdmin = chkRoleAdminOpt.Checked;
            string adminOpt = withAdmin ? " WITH ADMIN OPTION" : "";
            _dbService.ExecuteNonQuery($"GRANT {role} TO {user}{adminOpt}");
            LoadUserCurrentRoles();
            MessageBox.Show($"Gán role '{role}' cho user '{user}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("Lỗi gán role: " + ex.Message); }
    }

    private void btnRevokeRole_Click(object? sender, EventArgs e)
    {
        try
        {
            string user = cbRoleUser.SelectedItem?.ToString() ?? "";
            // Try to get selected role from the grid first
            string role = "";
            if (dgvUserCurrentRoles.SelectedRows.Count > 0)
                role = dgvUserCurrentRoles.SelectedRows[0].Cells["GRANTED_ROLE"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(role))
                role = cbRoleName.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(role))
            {
                ShowError("Vui lòng chọn User và chọn Role cần gỡ (chọn từ bảng hoặc dropdown).");
                return;
            }
            if (MessageBox.Show($"Gỡ role '{role}' khỏi user '{user}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            _dbService.ExecuteNonQuery($"REVOKE {role} FROM {user}");
            LoadUserCurrentRoles();
            MessageBox.Show($"Đã gỡ role '{role}' khỏi user '{user}'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("Lỗi gỡ role: " + ex.Message); }
    }

    private void ShowError(string msg) => MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
