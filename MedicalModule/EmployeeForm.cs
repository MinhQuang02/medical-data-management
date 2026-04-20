using System;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedicalDataManagement.MedicalModule;

public class EmployeeForm : Form
{
    private DatabaseService _db;
    private string _username;
    public string Schema { get; set; } = "QLBENHVIEN";

    // Uneditable Fields
    private TextBox txtMa = null!, txtHoTen = null!, txtPhai = null!, txtNgaySinh = null!, txtCMND = null!, txtVaiTro = null!, txtChuyenKhoa = null!;
    // Editable Fields
    private TextBox txtQueQuan = null!, txtSoDT = null!;
    private Button btnSave = null!;

    public EmployeeForm(DatabaseService db, string username)
    {
        _db = db;
        _username = username;
        this.Text = $"Employee Profile - {_username}";
        this.Size = new Size(500, 550);

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

        txtMa = AddRow("Mã NV:", txtX, ref y, true);
        txtHoTen = AddRow("Họ Tên:", txtX, ref y, true);
        txtPhai = AddRow("Phái:", txtX, ref y, true);
        txtNgaySinh = AddRow("Ngày Sinh:", txtX, ref y, true);
        txtCMND = AddRow("CMND:", txtX, ref y, true);
        txtVaiTro = AddRow("Vai Trò:", txtX, ref y, true);
        txtChuyenKhoa = AddRow("Chuyên Khoa:", txtX, ref y, true);

        y += 10;
        Label lblEdit = new Label() { Text = "Thông tin liên hệ (Có thể sửa)", Location = new Point(20, y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
        this.Controls.Add(lblEdit);
        y += 30;

        txtQueQuan = AddRow("Quê Quán:", txtX, ref y, false);
        txtSoDT = AddRow("Số Điện Thoại:", txtX, ref y, false);

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
            DataTable dt = _db.ExecuteQuery($"SELECT * FROM {Schema}.NHANVIEN WHERE MANV = :u", new[] { new OracleParameter("u", _username) });
            if (dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                txtMa.Text = r["MANV"].ToString();
                txtHoTen.Text = r["HOTEN"].ToString();
                txtPhai.Text = r["PHAI"].ToString();
                
                if (r["NGAYSINH"] != DBNull.Value)
                    txtNgaySinh.Text = Convert.ToDateTime(r["NGAYSINH"]).ToString("dd/MM/yyyy");
                
                txtCMND.Text = r["CMND"].ToString();
                txtVaiTro.Text = r["VAITRO"].ToString();
                txtChuyenKhoa.Text = r["CHUYENKHOA"].ToString();
                
                txtQueQuan.Text = r["QUEQUAN"].ToString();
                txtSoDT.Text = r["SODT"].ToString();
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu nhân viên.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading employee data: " + ex.Message);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _db.ExecuteNonQuery(
                $"UPDATE {Schema}.NHANVIEN SET QUEQUAN = :qq, SODT = :sdt WHERE MANV = :u",
                new OracleParameter[]
                {
                    new OracleParameter("qq", txtQueQuan.Text),
                    new OracleParameter("sdt", txtSoDT.Text),
                    new OracleParameter("u", _username)
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
