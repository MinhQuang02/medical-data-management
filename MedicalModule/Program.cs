namespace MedicalDataManagement.MedicalModule;



static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MedicalLoginForm());
    }
}
