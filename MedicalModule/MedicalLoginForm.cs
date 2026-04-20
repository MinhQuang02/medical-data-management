namespace MedicalDataManagement.MedicalModule;

public partial class MedicalLoginForm : Form
{
    public MedicalLoginForm()
    {
        InitializeComponent();
        // Use async void event handler so Oracle calls never block the UI thread
        btnLogin.Click += async (s, e) => await BtnLogin_ClickAsync();
    }

    private async Task BtnLogin_ClickAsync()
    {
        string username = txtUsername.Text.Trim().ToUpper();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username))
        {
            MessageBox.Show("Vui lòng nhập tên tài khoản.", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Disable button and show progress immediately (UI stays responsive)
        btnLogin.Enabled  = false;
        btnLogin.Text     = "Đang kết nối...";

        try
        {
            var db = new DatabaseService(username, password);

            // ── Step 1: Test connection on background thread ─────────────
            bool connected = await Task.Run(() => db.TestConnection());
            if (!connected)
            {
                MessageBox.Show(
                    "Kết nối thất bại!\n\nKiểm tra lại:\n" +
                    "  • Tên tài khoản (BS001 / DP001 / KTV001 / BN00001)\n" +
                    "  • Mật khẩu (mặc định: 123)\n" +
                    "  • Oracle xepdb1 đang chạy",
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnLogin.Text = "Đang xác thực quyền...";

            // ── Step 2: Fetch Oracle roles on background thread ───────────
            List<string> roles = await Task.Run(() => db.GetCurrentUserRoles());

            // ── Step 3: Route to correct form (runs on UI thread) ─────────
            Form targetForm;

            if      (roles.Contains("DIEUPHOIVIEN")) targetForm = new CoordinatorForm(db, username);
            else if (roles.Contains("BACSI"))         targetForm = new DoctorForm(db, username);
            else if (roles.Contains("KYTHUATVIEN"))   targetForm = new TechnicianForm(db, username);
            else if (roles.Contains("BENHNHAN"))      targetForm = new PatientForm(db, username);
            else                                       targetForm = new EmployeeForm(db, username);

            this.Hide();
            targetForm.FormClosed += (s, args) => this.Close();
            targetForm.Show();
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Split('\n')[0].Trim();
            MessageBox.Show(
                $"Đăng nhập thất bại:\n{msg}\n\n(Tài khoản: {username})",
                "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text    = "Đăng nhập";
        }
    }
}
