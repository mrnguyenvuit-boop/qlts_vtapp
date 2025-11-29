using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ClientPrinterTray
{
    public class WebSocketPrinterHub
    {
        private readonly PrintQueue _queue;

        public WebSocketPrinterHub(PrintQueue queue)
        {
            _queue = queue;
        }

        private async Task HandleConnection(WebSocket ws)
        {
            byte[] buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.CloseStatus.HasValue) break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var job = System.Text.Json.JsonSerializer.Deserialize<JobItem>(json);

                if (job != null && job.Files?.Count > 0)
                    await _queue.EnqueueAsync(job.Files);  // ✔ giờ chạy được
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }

        private class PrintCommand
        {
            public List<string>? files { get; set; }
        }
    }
}
