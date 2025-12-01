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

		public static async Task<bool> PrintSilentAsync(string file, string? printerName)
		{
			if (!File.Exists(file))
				throw new Exception($"File không tồn tại: {file}");

			if (!File.Exists(ExePath))
				throw new Exception("Không tìm thấy SumatraPDF.exe");

			if (string.IsNullOrWhiteSpace(printerName))
				printerName = new PrinterSettings().PrinterName;

			var psi = new ProcessStartInfo
			{
				FileName = ExePath,
				Arguments = $"-print-to \"{printerName}\" -silent \"{file}\"",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			using var process = Process.Start(psi)!;

			// ⏳ CHỜ 3–6s CHO SUMATRA XỬ LÝ PDF RỒI EXIT
			await Task.Run(() => process.WaitForExit(6000));

			return process.HasExited;
		}


		public static string[] GetPrinters()
        {
            var list = new string[PrinterSettings.InstalledPrinters.Count];
            PrinterSettings.InstalledPrinters.CopyTo(list, 0);
            return list;
        }
    }
}
