namespace MedicalDataManagement.AdminModule;

public partial class AdminDashboard : Form
{
    public AdminDashboard()
    {
        InitializeComponent();
        
        // Gắn chức năng cho nút RunScripts (ví dụ mình thêm một Button vào Form hoặc chạy ngầm nếu cần)
        // Nhưng tạo theo yêu cầu bạn: chạy script DB bằng C#. Tôi sẽ chạy ngầm ngay trên Load cho bạn kiểm chứng.
        this.Load += AdminDashboard_Load;
    }

    private void AdminDashboard_Load(object? sender, EventArgs e)
    {
        // Chạy Seed Data thẳng khi Form bật (Vì bạn muốn tự động xóa bảng/vẽ lại script)
        // Lưu ý: Đường dẫn thư mục scripts ở ngoài 1 cấp so với thư mục AdminModule
        string scriptsPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.StartupPath, @"..\..\..\..\scripts-database"));
        
        string seedResult = DatabaseSeeder.InitializeDatabase(scriptsPath);
        string debugResult = DebugDatabase.CheckHSBA_DVColumns();
        MessageBox.Show(seedResult + "\n\n" + debugResult, "Kết quả khởi tạo/seed CSDL Oracle", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
