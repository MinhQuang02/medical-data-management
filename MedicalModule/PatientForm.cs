using System;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.MedicalModule;

public class PatientForm : Form
{
    private DatabaseService _db;
    private string _username;
    public string Schema { get; set; } = "QLBENHVIEN";

    // Uneditable Fields
    private TextBox txtMa = null!, txtTen = null!, txtPhai = null!, txtNgaySinh = null!, txtCCCD = null!;
    // Editable Fields
    private TextBox txtSoNha = null!, txtDuong = null!, txtQuan = null!, txtTinh = null!;
    private Button btnSave = null!;

    public PatientForm(DatabaseService db, string username)
    {
        _db = db;
        _username = username;
        this.Text = $"Patient Profile - {_username}";
        this.Size = new Size(500, 500);

        InitializeUI();
        LoadData();
    }

    private void InitializeUI()
    {
        int y = 20;
        int txtX = 150;
        
        Label lblCore = new Label() { Text = "Thông tin cơ bản (Không thể sửa)", Location = new Point(20, y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
        this.Controls.Add(lblCore);
        y += 30;

        txtMa = AddRow("Mã BN:", txtX, ref y, true);
        txtTen = AddRow("Tên BN:", txtX, ref y, true);
        txtPhai = AddRow("Phái:", txtX, ref y, true);
        txtNgaySinh = AddRow("Ngày Sinh:", txtX, ref y, true);
        txtCCCD = AddRow("CCCD:", txtX, ref y, true);

        y += 10;
        Label lblEdit = new Label() { Text = "Thông tin liên hệ (Có thể sửa)", Location = new Point(20, y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
        this.Controls.Add(lblEdit);
        y += 30;

        txtSoNha = AddRow("Số Nhà:", txtX, ref y, false);
        txtDuong = AddRow("Tên Đường:", txtX, ref y, false);
        txtQuan = AddRow("Quận Huyện:", txtX, ref y, false);
        txtTinh = AddRow("Tỉnh/TP:", txtX, ref y, false);

        btnSave = new Button() { Text = "Lưu Thông Tin", Location = new Point(txtX, y + 10), Size = new Size(120, 30) };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);
    }

    private TextBox AddRow(string labelText, int txtX, ref int y, bool isReadOnly)
    {
        Label lbl = new Label() { Text = labelText, Location = new Point(20, y + 5), AutoSize = true };
        this.Controls.Add(lbl);

        TextBox txt = new TextBox() { Location = new Point(txtX, y), Size = new Size(250, 25), ReadOnly = isReadOnly };
        if (isReadOnly) txt.BackColor = Color.LightGray;
        this.Controls.Add(txt);

        y += 35;
        return txt;
    }

    private void LoadData()
    {
        try
        {
            DataTable dt = _db.ExecuteQuery($"SELECT * FROM {Schema}.BN_XEMTHONGTIN_BN");
            if (dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                txtMa.Text = r["MABN"].ToString();
                txtTen.Text = r["TENBN"].ToString();
                txtPhai.Text = r["PHAI"].ToString();
                
                if (r["NGAYSINH"] != DBNull.Value)
                    txtNgaySinh.Text = Convert.ToDateTime(r["NGAYSINH"]).ToString("dd/MM/yyyy");
                
                txtCCCD.Text = r["CCCD"].ToString();
                
                txtSoNha.Text = r["SONHA"].ToString();
                txtDuong.Text = r["TENDUONG"].ToString();
                txtQuan.Text = r["QUANHUYEN"].ToString();
                txtTinh.Text = r["TINHTP"].ToString();
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu bệnh nhân.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading patient data: " + ex.Message);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _db.ExecuteNonQuery(
                $"UPDATE {Schema}.BN_XEMTHONGTIN_BN SET SONHA = :sn, TENDUONG = :td, QUANHUYEN = :qh, TINHTP = :tp",
                new OracleParameter[]
                {
                    new OracleParameter("sn", txtSoNha.Text),
                    new OracleParameter("td", txtDuong.Text),
                    new OracleParameter("qh", txtQuan.Text),
                    new OracleParameter("tp", txtTinh.Text)
                });
            MessageBox.Show("Cập nhật thành công!");
            LoadData(); // reload to confirm
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cập nhật thất bại: " + ex.Message);
        }
    }
}
