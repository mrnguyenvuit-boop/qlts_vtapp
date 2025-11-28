using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace ClientPrinterTray
{
    public static class Printer
    {
        // ✅ App sẽ tìm SumatraPDF.exe ngay cạnh ClientPrinterTray.exe
        private static string ExePath =>
            Path.Combine(AppContext.BaseDirectory, "SumatraPDF.exe");

        public static void PrintSilent(string file, string? printerName)
        {
            if (!File.Exists(ExePath))
            {
                MessageBox.Show(
                    $"Không tìm thấy SumatraPDF.exe tại:\n{ExePath}",
                    "Lỗi in PDF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return; // ⬅️ Đừng throw nữa, để app không văng
            }

            if (string.IsNullOrWhiteSpace(printerName))
            {
                // Nếu chưa chọn thì dùng máy in mặc định của Windows
                printerName = new PrinterSettings().PrinterName;
            }

            var psi = new ProcessStartInfo
            {
                FileName = ExePath,
                Arguments = $"-print-to \"{printerName}\" -silent \"{file}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
        }

        public static string[] GetPrinters()
        {
            var list = new string[PrinterSettings.InstalledPrinters.Count];
            PrinterSettings.InstalledPrinters.CopyTo(list, 0);
            return list;
        }
    }
}
