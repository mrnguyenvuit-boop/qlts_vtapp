using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

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

        // Nhận list file → enqueue
        public async Task EnqueueAsync(List<string> files)
        {
            foreach (var f in files) _queue.Enqueue(f);
            await ProcessQueue();
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
                    Printer.PrintSilent(file, printer);

                    job.MarkSuccess();
                    _store.Add(PrintJob.From(job)); // 🔥 LƯU LOG

                    JobCompleted?.Invoke(file);
                    await SendSocket(new { type = "print-progress", file, index, total });
                }
                catch (Exception ex)
                {
                    job.MarkFail(ex.Message);
                    _store.Add(PrintJob.From(job)); // 🔥 LƯU LOG FAIL

                    PrintFailed?.Invoke(file);
                    await SendSocket(new { type = "print-error", file, message = ex.Message });
                    break;
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
