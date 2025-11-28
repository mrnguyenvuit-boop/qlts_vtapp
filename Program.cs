using ClientPrinterTray;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Ngăn chạy nhiều instance
        using Mutex mutex = new Mutex(true, "ClientPrinterTrayInstance", out bool isNew);
        if (!isNew)
        {
            MessageBox.Show("Ứng dụng đang chạy!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // DI container
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<AppSettings>();
                services.AddSingleton<JobStore>();
                services.AddSingleton<DbLogger>();    // 🔥 THÊM DÒNG NÀY
                services.AddSingleton<PrintQueue>();
                services.AddSingleton<PrintServer>();
                services.AddSingleton<MainForm>();
            })
            .Build();

        ApplicationConfiguration.Initialize();

        // Lấy MainForm từ DI và chạy
        var mainForm = host.Services.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
