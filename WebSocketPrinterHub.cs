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

        public async Task HandleConnection(WebSocket ws)
        {
            byte[] buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                if (result.CloseStatus.HasValue)
                    break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // 🔥 JSON từ web → convert thành JOB
                JobItem? job = JsonSerializer.Deserialize<JobItem>(json);

                if (job != null)
                    await _queue.EnqueueAsync(job);  // đưa vào hàng đợi in
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}
