using System;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace MedicalDataManagement.MedicalModule;

public class TechnicianForm : Form
{
    private DatabaseService _db;
    private string _username;
    public string Schema { get; set; } = "QLBENHVIEN";

    private DataGridView dgvServices = null!;
    private TextBox txtResult = null!;
    private Button btnSave = null!;

    private TextBox txtQueQuan = null!;
    private TextBox txtSoDT = null!;
    private Button btnSavePers = null!;

    public TechnicianForm(DatabaseService db, string username)
    {
        _db = db;
        _username = username;
        this.Text = "Technician Form - Assigned Services";
        this.Size = new Size(800, 600);
        
        InitializeUI();
        LoadData();
    }

    private void InitializeUI()
    {
        Label lblTitle = new Label() { Text = "Các Dịch Vụ Được Phân Công", Location = new Point(20, 20), AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold) };
        this.Controls.Add(lblTitle);

        dgvServices = new DataGridView() { Location = new Point(20, 50), Size = new Size(740, 300), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
        this.Controls.Add(dgvServices);
        
        Label lblResult = new Label() { Text = "Kết quả:", Location = new Point(20, 360), AutoSize = true };
        this.Controls.Add(lblResult);
        
        txtResult = new TextBox() { Location = new Point(80, 355), Size = new Size(400, 30) };
        this.Controls.Add(txtResult);
        
        btnSave = new Button() { Text = "Lưu Kết Quả", Location = new Point(490, 350), Size = new Size(120, 30) };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);

        // Personal Info Section
        Label lblPers = new Label() { Text = "Thông Tin Cá Nhân", Location = new Point(20, 420), AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold) };
        this.Controls.Add(lblPers);

        Label lblQQ = new Label() { Text = "Quê Quán:", Location = new Point(20, 460), AutoSize = true };
        this.Controls.Add(lblQQ);
        txtQueQuan = new TextBox() { Location = new Point(100, 455), Size = new Size(200, 30) };
        this.Controls.Add(txtQueQuan);

        Label lblSDT = new Label() { Text = "SĐT:", Location = new Point(320, 460), AutoSize = true };
        this.Controls.Add(lblSDT);
        txtSoDT = new TextBox() { Location = new Point(360, 455), Size = new Size(150, 30) };
        this.Controls.Add(txtSoDT);

        btnSavePers = new Button() { Text = "Cập Nhật", Location = new Point(530, 450), Size = new Size(100, 30) };
        btnSavePers.Click += BtnSavePers_Click;
        this.Controls.Add(btnSavePers);
    }

    private void LoadData()
    {
        try
        {
            // RBAC View limits naturally to current user
            dgvServices.DataSource = _db.ExecuteQuery($"SELECT * FROM {Schema}.KTV_XEMHSBA_DV");

            System.Data.DataTable dt = _db.ExecuteQuery($"SELECT * FROM {Schema}.KTV_XEMTTCANHAN");
            if (dt.Rows.Count > 0)
            {
                txtQueQuan.Text = dt.Rows[0]["QUEQUAN"].ToString();
                txtSoDT.Text = dt.Rows[0]["SODT"].ToString();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading assigned services or personal info: " + ex.Message);
        }
    }

    private void BtnSavePers_Click(object? sender, EventArgs e)
    {
        try
        {
            _db.ExecuteNonQuery(
                $"UPDATE {Schema}.KTV_XEMTTCANHAN SET QUEQUAN = :qq, SODT = :sdt",
                new OracleParameter[]
                {
                    new OracleParameter("qq", txtQueQuan.Text),
                    new OracleParameter("sdt", txtSoDT.Text)
                });
            MessageBox.Show("Cập nhật thông tin cá nhân thành công!");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cập nhật thất bại: " + ex.Message);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (dgvServices.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một dòng dịch vụ.");
            return;
        }

        var row = dgvServices.SelectedRows[0];
        string ma = row.Cells["MAHSBA"].Value.ToString()!;
        string loai = row.Cells["LOAIDV"].Value.ToString()!;

        try
        {
            _db.ExecuteNonQuery(
                $"UPDATE {Schema}.KTV_XEMHSBA_DV SET KETQUA = :kq WHERE MAHSBA = :ma AND LOAIDV = :loai",
                new OracleParameter[]
                {
                    new OracleParameter("kq", txtResult.Text),
                    new OracleParameter("ma", ma),
                    new OracleParameter("loai", loai)
                });
            MessageBox.Show("Đã lưu (và Oracle Audit sẽ ghi lại cập nhật này)!");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cập nhật thất bại: " + ex.Message);
        }
    }
}
