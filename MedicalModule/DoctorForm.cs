using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.MedicalModule;

public partial class DoctorForm : Form
{
    private DatabaseService _db;
    private string _username;
    public string Schema { get; set; } = "QLBENHVIEN";

    private Label lblStatus = null!;

    public DoctorForm(DatabaseService db, string username)
    {
        InitializeComponent();
        _db = db;
        _username = username;

        lblStatus = new Label
        {
            Text = "⏳ Đang tải dữ liệu...",
            Dock = DockStyle.Bottom,
            Height = 24,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray
        };
        this.Controls.Add(lblStatus);

        // Load data after form is rendered — never block UI thread
        this.Shown += async (s, e) => await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        lblStatus.Text = "⏳ Đang tải dữ liệu...";
        // VPD NV_VPD  : BS% sees only their own row in NHANVIEN
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.NHANVIEN"),  dgvNhanVien, "Nhân Viên");
        // VPD BS_VPD  : BS% sees only HSBA where MABS = USER
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA"),      dgvHSBA,     "Hồ Sơ Bệnh Án");
        // VPD BS_HSBENHNHAN: BS% sees only patients in their treated HSBA
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.BENHNHAN"),  dgvBenhNhan, "Bệnh Nhân");
        // VPD BS_HSBA linked to DONTHUOC via MAHSBA
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.DONTHUOC"),  dgvDonThuoc, "Đơn Thuốc");
        // NOTE: Doctor has INSERT+DELETE on HSBA_DV but NOT SELECT — no grid load
        lblStatus.Text = "✅ Tải xong.";
    }

    private async Task SafeLoadAsync(Func<DataTable> query, DataGridView dgv, string section)
    {
        try
        {
            DataTable dt = await Task.Run(query);
            dgv.DataSource = dt;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải [{section}]: {ex.Message.Split('\n')[0]}",
                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ─── Nhân Viên ────────────────────────────────────────────────
    private async void btnUpdateNV_Click(object sender, EventArgs e)
    {
        if (dgvNhanVien.CurrentRow == null) return;
        var row = dgvNhanVien.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.NHANVIEN SET QUEQUAN=:qq, SODT=:sdt WHERE MANV=:id",
                new OracleParameter[] { new("qq",row.Cells["QUEQUAN"].Value), new("sdt",row.Cells["SODT"].Value), new("id",row.Cells["MANV"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.NHANVIEN"), dgvNhanVien, "Nhân Viên");
            MessageBox.Show("Cập nhật thông tin cá nhân thành công!");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── HSBA ────────────────────────────────────────────────────
    private async void btnUpdateHSBA_Click(object sender, EventArgs e)
    {
        if (dgvHSBA.CurrentRow == null) return;
        var r = dgvHSBA.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.HSBA SET CHANDOAN=:cd, DIEUTRI=:dt, KETLUAN=:kl WHERE MAHSBA=:id",
                new OracleParameter[] { new("cd",r.Cells["CHANDOAN"].Value), new("dt",r.Cells["DIEUTRI"].Value), new("kl",r.Cells["KETLUAN"].Value), new("id",r.Cells["MAHSBA"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA"), dgvHSBA, "Hồ Sơ Bệnh Án");
            MessageBox.Show("Cập nhật HSBA thành công! (Oracle Audit đã ghi vết)");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── Bệnh Nhân ───────────────────────────────────────────────
    private async void btnUpdateBN_Click(object sender, EventArgs e)
    {
        if (dgvBenhNhan.CurrentRow == null) return;
        var r = dgvBenhNhan.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.BENHNHAN SET TIENSUBENH=:ts, TIENSUBENHGD=:gd, DIUNGTHUOC=:du WHERE MABN=:id",
                new OracleParameter[] { new("ts",r.Cells["TIENSUBENH"].Value), new("gd",r.Cells["TIENSUBENHGD"].Value), new("du",r.Cells["DIUNGTHUOC"].Value), new("id",r.Cells["MABN"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.BENHNHAN"), dgvBenhNhan, "Bệnh Nhân");
            MessageBox.Show("Cập nhật bệnh nhân thành công!");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── Đơn Thuốc ───────────────────────────────────────────────
    private async void btnAddDT_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtMaHSBA.Text)) return;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"INSERT INTO {Schema}.DONTHUOC VALUES (:ma, SYSDATE, :ten, :lieu)",
                new OracleParameter[] { new("ma",txtMaHSBA.Text), new("ten",txtTenThuoc.Text), new("lieu",txtLieuDung.Text) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.DONTHUOC"), dgvDonThuoc, "Đơn Thuốc");
            MessageBox.Show("Thêm đơn thuốc thành công!");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    private async void btnUpdateDT_Click(object sender, EventArgs e)
    {
        if (dgvDonThuoc.CurrentRow == null) return;
        var r = dgvDonThuoc.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.DONTHUOC SET TENTHUOC=:tt, LIEUDUNG=:lieu WHERE MAHSBA=:ma AND NGAYDT=:ng AND TENTHUOC=:old",
                new OracleParameter[] { new("tt",txtTenThuoc.Text), new("lieu",txtLieuDung.Text), new("ma",r.Cells["MAHSBA"].Value), new("ng",r.Cells["NGAYDT"].Value), new("old",r.Cells["TENTHUOC"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.DONTHUOC"), dgvDonThuoc, "Đơn Thuốc");
            MessageBox.Show("Cập nhật đơn thuốc thành công! (Oracle Audit đã ghi vết)");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    private void dgvDonThuoc_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (dgvDonThuoc.CurrentRow == null) return;
        var r = dgvDonThuoc.CurrentRow;
        txtMaHSBA.Text   = r.Cells["MAHSBA"].Value?.ToString();
        txtTenThuoc.Text = r.Cells["TENTHUOC"].Value?.ToString();
        txtLieuDung.Text = r.Cells["LIEUDUNG"].Value?.ToString();
    }

    private async void btnDeleteDT_Click(object sender, EventArgs e)
    {
        if (dgvDonThuoc.CurrentRow == null) return;
        var r = dgvDonThuoc.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"DELETE FROM {Schema}.DONTHUOC WHERE MAHSBA=:ma AND NGAYDT=:ng",
                new OracleParameter[] { new("ma",r.Cells["MAHSBA"].Value), new("ng",r.Cells["NGAYDT"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.DONTHUOC"), dgvDonThuoc, "Đơn Thuốc");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── HSBA_DV (Doctor: INSERT + DELETE only, no SELECT) ────────
    private async void btnAddDichVu_Click(object sender, EventArgs e)
    {
        if (dgvHSBA.CurrentRow == null) { MessageBox.Show("Chọn một hồ sơ bệnh án trước."); return; }
        if (string.IsNullOrWhiteSpace(txtLoaiDV.Text)) { MessageBox.Show("Nhập loại dịch vụ."); return; }
        var r = dgvHSBA.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"INSERT INTO {Schema}.HSBA_DV VALUES (:ma, :loai, SYSDATE, NULL, NULL)",
                new OracleParameter[] { new("ma",r.Cells["MAHSBA"].Value), new("loai",txtLoaiDV.Text) }));
            MessageBox.Show("Đã thêm dịch vụ thành công!");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    private async void btnDeleteDV_Click(object sender, EventArgs e)
    {
        if (dgvDichVu.CurrentRow == null) return;
        var r = dgvDichVu.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"DELETE FROM {Schema}.HSBA_DV WHERE MAHSBA=:ma AND LOAIDV=:loai",
                new OracleParameter[] { new("ma",r.Cells["MAHSBA"].Value), new("loai",r.Cells["LOAIDV"].Value) }));
            MessageBox.Show("Đã xoá dịch vụ!");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }
}