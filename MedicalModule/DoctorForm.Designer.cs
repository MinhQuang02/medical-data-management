namespace MedicalDataManagement.MedicalModule
{
    partial class DoctorForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;

        private Button btnNavProfile, btnNavHSBA, btnNavBN, btnNavDT, btnNavDV;

        private TabControl tabControl;
        private TabPage tabNhanVien, tabHSBA, tabBenhNhan, tabDonThuoc, tabDichVu;

        private DataGridView dgvNhanVien, dgvHSBA, dgvBenhNhan, dgvDonThuoc, dgvDichVu;

        private Button btnUpdateNV, btnUpdateHSBA, btnUpdateBN;
        private Button btnAddDT, btnUpdateDT, btnDeleteDT;
        private Button btnAddDV, btnDeleteDV;

        private TextBox txtMaHSBA, txtTenThuoc, txtLieuDung;
        private TextBox txtLoaiDV, txtMaKTV, txtKetQua;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelSidebar = new Panel();
            panelHeader = new Panel();
            panelContent = new Panel();

            tabControl = new TabControl();

            tabNhanVien = new TabPage();
            tabHSBA = new TabPage();
            tabBenhNhan = new TabPage();
            tabDonThuoc = new TabPage();
            tabDichVu = new TabPage();

            SuspendLayout();

            // ================= SIDEBAR =================
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 220;
            panelSidebar.BackColor = Color.FromArgb(28, 35, 49);

            Label lbl = new Label()
            {
                Text = "DOCTOR",
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            panelSidebar.Controls.Add(lbl);

            int top = 80;
            btnNavProfile = StyleNav(" Cá nhân", top, () => tabControl.SelectedTab = tabNhanVien); top += 50;
            btnNavHSBA = StyleNav(" Hồ sơ", top, () => tabControl.SelectedTab = tabHSBA); top += 50;
            btnNavBN = StyleNav(" Bệnh nhân", top, () => tabControl.SelectedTab = tabBenhNhan); top += 50;
            btnNavDT = StyleNav(" Đơn thuốc", top, () => tabControl.SelectedTab = tabDonThuoc); top += 50;
            btnNavDV = StyleNav(" Dịch vụ", top, () => tabControl.SelectedTab = tabDichVu);

            panelSidebar.Controls.AddRange(new Control[]
            {
                btnNavProfile, btnNavHSBA, btnNavBN, btnNavDT, btnNavDV
            });

            // ================= HEADER =================
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 60;
            panelHeader.BackColor = Color.White;

            Label title = new Label()
            {
                Text = "Doctor Dashboard",
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
                tabNhanVien, tabHSBA, tabBenhNhan, tabDonThuoc, tabDichVu
            });

            // ================= TAB NHAN VIEN =================
            tabNhanVien.BackColor = Color.FromArgb(244, 246, 249);

            dgvNhanVien = CreateGrid(25, 25, 900, 350);
            dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            btnUpdateNV = CreateButton("Cập nhật", 25, 390);
            btnUpdateNV.Click += btnUpdateNV_Click;

            tabNhanVien.Controls.AddRange(new Control[]
            {
                dgvNhanVien, btnUpdateNV
            });

            // ================= TAB HSBA =================
            tabHSBA.BackColor = Color.FromArgb(244, 246, 249);

            dgvHSBA = CreateGrid(25, 25, 900, 350);
            dgvHSBA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            btnUpdateHSBA = CreateButton("Cập nhật HSBA", 25, 390);
            btnUpdateHSBA.Click += btnUpdateHSBA_Click;

            tabHSBA.Controls.AddRange(new Control[]
            {
                dgvHSBA, btnUpdateHSBA
            });

            // ================= TAB BENH NHAN =================
            tabBenhNhan.BackColor = Color.FromArgb(244, 246, 249);

            dgvBenhNhan = CreateGrid(25, 25, 900, 350);
            dgvBenhNhan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            btnUpdateBN = CreateButton("Cập nhật BN", 25, 390);
            btnUpdateBN.Click += btnUpdateBN_Click;

            tabBenhNhan.Controls.AddRange(new Control[]
            {
                dgvBenhNhan, btnUpdateBN
            });

            // ================= TAB DON THUOC =================
            tabDonThuoc.BackColor = Color.FromArgb(244, 246, 249);

            dgvDonThuoc = CreateGrid(25, 25, 900, 250);
            dgvDonThuoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDonThuoc.CellClick += dgvDonThuoc_CellClick;

            txtMaHSBA = CreateTextBox(25, 300, 150, "Mã HSBA");
            txtTenThuoc = CreateTextBox(185, 300, 200, "Tên thuốc");
            txtLieuDung = CreateTextBox(395, 300, 200, "Liều dùng");

            btnAddDT = CreateButton("Thêm", 25, 350);
            btnUpdateDT = CreateButton("Sửa", 140, 350);
            btnDeleteDT = CreateButton("Xóa", 255, 350);

            btnAddDT.Click += btnAddDT_Click;
            btnUpdateDT.Click += btnUpdateDT_Click;
            btnDeleteDT.Click += btnDeleteDT_Click;

            tabDonThuoc.Controls.AddRange(new Control[]
            {
                dgvDonThuoc,
                txtMaHSBA, txtTenThuoc, txtLieuDung,
                btnAddDT, btnUpdateDT, btnDeleteDT
            });

            // ================= TAB DICH VU =================
            tabDichVu.BackColor = Color.FromArgb(244, 246, 249);

            dgvDichVu = CreateGrid(25, 25, 900, 250);
            dgvDichVu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            txtLoaiDV = CreateTextBox(25, 300, 180, "Loại DV");
            txtMaKTV = CreateTextBox(215, 300, 150, "Mã KTV");
            txtKetQua = CreateTextBox(375, 300, 250, "Kết quả");

            btnAddDV = CreateButton("Thêm DV", 25, 350);
            btnDeleteDV = CreateButton("Xóa DV", 140, 350);

            btnAddDV.Click += btnAddDichVu_Click;
            btnDeleteDV.Click += btnDeleteDV_Click;

            tabDichVu.Controls.AddRange(new Control[]
            {
                dgvDichVu,
                txtLoaiDV, txtMaKTV, txtKetQua,
                btnAddDV, btnDeleteDV
            });

            // ================= FORM =================
            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);

            this.ClientSize = new Size(1200, 700);
            this.Text = "Doctor Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;

            ResumeLayout(false);
        }

        // ================= STYLE =================
        private Button StyleNav(string text, int top, Action action)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = new Point(0, top),
                Size = new Size(220, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();

            return btn;
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
                Padding = new Padding(10, 5, 10, 5),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 45),
            };
        }
    }
}