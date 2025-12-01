using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.IO;
using System.Net.Http;

namespace ClientPrinterTray
{
	public class PrintQueue
	{
		private readonly ConcurrentQueue<string> _queue = new();
		private bool _printing = false;

		private readonly AppSettings _settings;
		private readonly JobStore _store;

		public WebSocket? Socket { get; set; }

		// EVENTS
		public event Action<string>? JobCompleted;
		public event Action<int>? PrintFinished;
		public event Action<string>? PrintFailed;

		public PrintQueue(AppSettings settings, JobStore store)
		{
			_settings = settings;
			_store = store;
		}

		// Nhận URL → tải về temp → thêm vào queue
		public async Task EnqueueAsync(List<string> files)
		{
			foreach (var url in files)
			{
				string localFile = await DownloadPdfToTempAsync(url);
				_queue.Enqueue(localFile);
			}

			await ProcessQueue();
		}

		public static async Task<string> DownloadPdfToTempAsync(string url)
		{
			string temp = Path.Combine(Path.GetTempPath(), Path.GetFileName(url));

			using var client = new HttpClient();
			var data = await client.GetByteArrayAsync(url);

			await File.WriteAllBytesAsync(temp, data);

			return temp;
		}

		private async Task ProcessQueue()
		{
			if (_printing) return;
			_printing = true;

			string printer = _settings.DefaultPrinter;
			int total = _queue.Count, index = 0;

			while (_queue.TryDequeue(out var file))
			{
				index++;

				var job = new JobItem()
				{
					Printer = printer,
					Files = new List<string>() { file }
				};

				try
				{
					bool ok = await Printer.PrintSilentAsync(file, printer); // 🔥 CHỜ TIẾN TRÌNH IN

					if (!ok)
						throw new Exception("Quá thời gian xử lý PDF, có thể máy in bận / lỗi.");

					job.MarkSuccess();
					_store.Add(PrintJob.From(job));

					JobCompleted?.Invoke(file);
					await SendSocket(new { type = "print-progress", file, index, total });
				}
				catch (Exception ex)
				{
					job.MarkFail(ex.Message);
					_store.Add(PrintJob.From(job));

					PrintFailed?.Invoke(file);
					await SendSocket(new { type = "print-error", file, message = ex.Message });
				}
				finally
				{
					try
					{
						if (File.Exists(file))
							File.Delete(file);  // 🟢 XÓA KHI TIẾN TRÌNH ĐÃ EXIT
					}
					catch { }
				}

				await Task.Delay(200);
			}

			PrintFinished?.Invoke(total);
			await SendSocket(new { type = "print-completed", total });

			_printing = false;
		}

		private async Task SendSocket(object data)
		{
			if (Socket == null) return;

			var bytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data));
			try
			{
				await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, default);
			}
			catch { Socket = null; }
		}
	}
}
