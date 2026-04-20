using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.MedicalModule;

public partial class CoordinatorForm : Form
{
    private DatabaseService _db;
    private string _username;
    public string Schema { get; set; } = "QLBENHVIEN";

    private Label lblStatus = null!;

    public CoordinatorForm(DatabaseService db, string username)
    {
        InitializeComponent();
        _db = db;
        _username = username;

        // Status label shown while loading
        lblStatus = new Label
        {
            Text = "⏳ Đang tải dữ liệu...",
            Dock = DockStyle.Bottom,
            Height = 24,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray
        };
        this.Controls.Add(lblStatus);

        // Load AFTER the form is shown so UI thread is never blocked
        this.Shown += async (s, e) => await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        lblStatus.Text = "⏳ Đang tải dữ liệu...";
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.NHANVIEN"),  dgvNhanVien, "Nhân Viên");
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.BENHNHAN"),  dgvBenhNhan, "Bệnh Nhân");
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA"),      dgvHSBA,     "Hồ Sơ Bệnh Án");
        await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA_DV"),   dgvDichVu,   "Dịch Vụ");
        lblStatus.Text = "✅ Tải xong.";
    }

    private async Task SafeLoadAsync(Func<DataTable> query, DataGridView dgv, string section)
    {
        try
        {
            // Execute Oracle query on background thread
            DataTable dt = await Task.Run(query);
            // Marshal back to UI thread to set DataSource
            dgv.DataSource = dt;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải [{section}]: {ex.Message.Split('\n')[0]}",
                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ─── Nhân Viên ───────────────────────────────────────────────
    private async void btnUpdateNV_Click(object sender, EventArgs e)
    {
        if (dgvNhanVien.CurrentRow == null) return;
        var r = dgvNhanVien.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.NHANVIEN SET QUEQUAN = :qq, SODT = :sdt WHERE MANV = :id",
                new OracleParameter[] { new("qq", r.Cells["QUEQUAN"].Value), new("sdt", r.Cells["SODT"].Value), new("id", r.Cells["MANV"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.NHANVIEN"), dgvNhanVien, "Nhân Viên");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── Bệnh Nhân ───────────────────────────────────────────────
    private async void btnAddBN_Click(object sender, EventArgs e)
    {
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $@"INSERT INTO {Schema}.BENHNHAN
                   (MABN,TENBN,PHAI,NGAYSINH,CCCD,SONHA,TENDUONG,QUANHUYEN,TINHTP,TIENSUBENH,TIENSUBENHGD,DIUNGTHUOC)
                   VALUES (:ma,:ten,:phai,SYSDATE,:cccd,:sonha,:duong,:qh,:tp,NULL,NULL,NULL)",
                new OracleParameter[]
                {
                    new("ma",txtMaBN.Text), new("ten",txtTenBN.Text), new("phai",txtPhai.Text),
                    new("cccd",txtCCCD.Text), new("sonha",txtSoNha.Text), new("duong",txtDuong.Text),
                    new("qh",txtQuan.Text), new("tp",txtTP.Text)
                }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.BENHNHAN"), dgvBenhNhan, "Bệnh Nhân");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    private async void btnUpdateBN_Click(object sender, EventArgs e)
    {
        if (dgvBenhNhan.CurrentRow == null) return;
        var r = dgvBenhNhan.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.BENHNHAN SET SONHA=:sn,TENDUONG=:td,QUANHUYEN=:qh,TINHTP=:tp WHERE MABN=:id",
                new OracleParameter[] { new("sn",r.Cells["SONHA"].Value), new("td",r.Cells["TENDUONG"].Value), new("qh",r.Cells["QUANHUYEN"].Value), new("tp",r.Cells["TINHTP"].Value), new("id",r.Cells["MABN"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.BENHNHAN"), dgvBenhNhan, "Bệnh Nhân");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── HSBA ────────────────────────────────────────────────────
    private async void btnAddHSBA_Click(object sender, EventArgs e)
    {
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $@"INSERT INTO {Schema}.HSBA (MAHSBA,MABN,NGAY,CHANDOAN,DIEUTRI,MABS,MAKHOA,KETLUAN)
                   VALUES (:ma,:mabn,SYSDATE,NULL,NULL,:mabs,:makhoa,NULL)",
                new OracleParameter[] { new("ma",txtMaHSBA.Text), new("mabn",txtMaBN.Text), new("mabs",txtMaBS.Text), new("makhoa",txtMaKhoa.Text) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA"), dgvHSBA, "Hồ Sơ Bệnh Án");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    private async void btnUpdateHSBA_Click(object sender, EventArgs e)
    {
        if (dgvHSBA.CurrentRow == null) return;
        var r = dgvHSBA.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.HSBA SET MAKHOA=:mk, MABS=:bs WHERE MAHSBA=:id",
                new OracleParameter[] { new("mk",r.Cells["MAKHOA"].Value), new("bs",r.Cells["MABS"].Value), new("id",r.Cells["MAHSBA"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA"), dgvHSBA, "Hồ Sơ Bệnh Án");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }

    // ─── Dịch Vụ ──────────────────────────────────────────────────
    private async void btnUpdateDV_Click(object sender, EventArgs e)
    {
        if (dgvDichVu.CurrentRow == null) return;
        var r = dgvDichVu.CurrentRow;
        try
        {
            await Task.Run(() => _db.ExecuteNonQuery(
                $"UPDATE {Schema}.HSBA_DV SET MAKTV=:ktv WHERE MAHSBA=:ma AND LOAIDV=:loai AND NGAYDV=:ngay",
                new OracleParameter[] { new("ktv",r.Cells["MAKTV"].Value), new("ma",r.Cells["MAHSBA"].Value), new("loai",r.Cells["LOAIDV"].Value), new("ngay",r.Cells["NGAYDV"].Value) }));
            await SafeLoadAsync(() => _db.ExecuteQuery($"SELECT * FROM {Schema}.HSBA_DV"), dgvDichVu, "Dịch Vụ");
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message.Split('\n')[0]); }
    }
}