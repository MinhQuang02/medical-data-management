namespace MedicalDataManagement.MedicalModule
{
    partial class CoordinatorForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;

        private Button btnNavNhanVien;
        private Button btnNavBenhNhan;
        private Button btnNavHSBA;
        private Button btnNavDichVu;

        private TabControl tabControl;
        private TabPage tabNhanVien, tabBenhNhan, tabHSBA, tabDichVu;

        private DataGridView dgvNhanVien, dgvBenhNhan, dgvHSBA, dgvDichVu;

        private Button btnUpdateNV;
        private Button btnAddBN, btnUpdateBN;
        private Button btnAddHSBA, btnUpdateHSBA;
        private Button btnUpdateDV;

        private TextBox txtMaBN, txtTenBN, txtPhai, txtCCCD;
        private TextBox txtSoNha, txtDuong, txtQuan, txtTP;

        private TextBox txtMaHSBA, txtMaBS, txtMaKhoa;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new Panel();
            this.panelHeader = new Panel();
            this.panelContent = new Panel();

            this.tabControl = new TabControl();
            this.tabNhanVien = new TabPage();
            this.tabBenhNhan = new TabPage();
            this.tabHSBA = new TabPage();
            this.tabDichVu = new TabPage();

            this.btnNavNhanVien = new Button();
            this.btnNavBenhNhan = new Button();
            this.btnNavHSBA = new Button();
            this.btnNavDichVu = new Button();

            this.SuspendLayout();

            // ================= SIDEBAR =================
            panelSidebar.BackColor = Color.FromArgb(28, 35, 49);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 220;

            Label lbl = new Label()
            {
                Text = "COORDINATOR",
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            panelSidebar.Controls.Add(lbl);

            int top = 80;
            StyleNav(btnNavNhanVien, " Nhân viên", top); top += 50;
            StyleNav(btnNavBenhNhan, " Bệnh nhân", top); top += 50;
            StyleNav(btnNavHSBA, " Hồ sơ", top); top += 50;
            StyleNav(btnNavDichVu, " Dịch vụ", top);

            panelSidebar.Controls.AddRange(new Control[]
            {
                btnNavNhanVien, btnNavBenhNhan, btnNavHSBA, btnNavDichVu
            });

            // ================= HEADER =================
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 60;
            panelHeader.BackColor = Color.White;

            Label title = new Label()
            {
                Text = "Coordinator Dashboard",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 20),
                AutoSize = true
            };

            panelHeader.Controls.Add(title);

            // ================= CONTENT =================
            panelContent.Dock = DockStyle.Fill;
            panelContent.Controls.Add(tabControl);

            // ================= TABCONTROL =================
            tabControl.Dock = DockStyle.Fill;
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;

            tabControl.Controls.AddRange(new Control[]
            {
                tabNhanVien, tabBenhNhan, tabHSBA, tabDichVu
            });

            // ================= NHÂN VIÊN =================
            tabNhanVien.BackColor = Color.FromArgb(244, 246, 249);

            dgvNhanVien = CreateGrid(25, 25, 900, 300);
            dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            btnUpdateNV = CreateButton("Cập nhật", 25, 340);
            btnUpdateNV.Click += btnUpdateNV_Click;

            tabNhanVien.Controls.AddRange(new Control[]
            {
                dgvNhanVien, btnUpdateNV
            });

            // ================= BỆNH NHÂN =================
            tabBenhNhan.BackColor = Color.FromArgb(244, 246, 249);

            dgvBenhNhan = CreateGrid(25, 25, 900, 250);
            dgvBenhNhan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            int y = 300;

            txtMaBN = CreateTextBox(25, y, 150, "Mã BN");
            txtTenBN = CreateTextBox(185, y, 200, "Tên BN");
            txtPhai = CreateTextBox(395, y, 100, "Phái");
            txtCCCD = CreateTextBox(505, y, 180, "CCCD");

            txtSoNha = CreateTextBox(25, y + 40, 150, "Số nhà");
            txtDuong = CreateTextBox(185, y + 40, 200, "Đường");
            txtQuan = CreateTextBox(395, y + 40, 150, "Quận");
            txtTP = CreateTextBox(555, y + 40, 150, "Tỉnh");

            btnAddBN = CreateButton("Thêm", 25, y + 90);
            btnUpdateBN = CreateButton("Cập nhật", 140, y + 90);

            btnAddBN.Click += btnAddBN_Click;
            btnUpdateBN.Click += btnUpdateBN_Click;

            tabBenhNhan.Controls.AddRange(new Control[]
            {
                dgvBenhNhan,
                txtMaBN, txtTenBN, txtPhai, txtCCCD,
                txtSoNha, txtDuong, txtQuan, txtTP,
                btnAddBN, btnUpdateBN
            });

            // ================= HSBA =================
            tabHSBA.BackColor = Color.FromArgb(244, 246, 249);

            dgvHSBA = CreateGrid(25, 25, 900, 250);
            dgvHSBA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            txtMaHSBA = CreateTextBox(25, 300, 150, "Mã HSBA");
            txtMaBS = CreateTextBox(185, 300, 150, "Mã BS");
            txtMaKhoa = CreateTextBox(345, 300, 150, "Mã Khoa");

            btnAddHSBA = CreateButton("Thêm HSBA", 25, 350);
            btnUpdateHSBA = CreateButton("Cập nhật", 150, 350);

            btnAddHSBA.Click += btnAddHSBA_Click;
            btnUpdateHSBA.Click += btnUpdateHSBA_Click;

            tabHSBA.Controls.AddRange(new Control[]
            {
                dgvHSBA,
                txtMaHSBA, txtMaBS, txtMaKhoa,
                btnAddHSBA, btnUpdateHSBA
            });

            // ================= DỊCH VỤ =================
            tabDichVu.BackColor = Color.FromArgb(244, 246, 249);

            dgvDichVu = CreateGrid(25, 25, 900, 300);
            dgvDichVu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            btnUpdateDV = CreateButton("Cập nhật KTV", 25, 340);
            btnUpdateDV.Click += btnUpdateDV_Click;

            tabDichVu.Controls.AddRange(new Control[]
            {
                dgvDichVu, btnUpdateDV
            });

            // ================= FORM =================
            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);

            this.ClientSize = new Size(1200, 700);
            this.Text = "Coordinator Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;

            this.ResumeLayout(false);
        }

        // ================= STYLE =================
        private void StyleNav(Button btn, string text, int top)
        {
            btn.Text = text;
            btn.Location = new Point(0, top);
            btn.Size = new Size(220, 45);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Click += (s, e) =>
            {
                if (text.Contains("Nhân")) tabControl.SelectedTab = tabNhanVien;
                else if (text.Contains("Bệnh")) tabControl.SelectedTab = tabBenhNhan;
                else if (text.Contains("Hồ")) tabControl.SelectedTab = tabHSBA;
                else tabControl.SelectedTab = tabDichVu;
            };
        }

        private DataGridView CreateGrid(int x, int y, int w, int h)
        {
            return new DataGridView()
            {
                Location = new Point(x, y),
                Size = new Size(w, h)
            };
        }

        private TextBox CreateTextBox(int x, int y, int w, string placeholder)
        {
            return new TextBox()
            {
                Location = new Point(x, y),
                Size = new Size(w, 30),
                PlaceholderText = placeholder
            };
        }

        private Button CreateButton(string text, int x, int y)
        {
            return new Button()
            {
                Text = " " + text,
                Left = x,
                Top = y,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10,5,10,5),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30,30,45),
            };
        }
    }
}