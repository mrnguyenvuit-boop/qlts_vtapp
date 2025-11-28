using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientPrinterTray
{
    public class PrintServer
    {
        private readonly PrintQueue _queue;
        private readonly AppSettings _settings;
        private bool _running = false;
        private HttpListener? _listener;

        public PrintServer(PrintQueue queue, AppSettings settings)
        {
            _queue = queue;
            _settings = settings;
        }

        public async void Start()
        {
            if (_running) return;
            _running = true;

            string url = $"http://localhost:{_settings.Port}/ws/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _listener.Start();

            Console.WriteLine($"🔥 WebSocket server running at {url}");

            _ = Task.Run(async () =>
            {
                while (_running)
                {
                    var context = await _listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                        _ = HandleWebSocket(context);
                    else
                        context.Response.Close();
                }
            });
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }

        private async Task HandleWebSocket(HttpListenerContext context)
        {
            var ws = (await context.AcceptWebSocketAsync(null)).WebSocket;

            byte[] buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.CloseStatus.HasValue) break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Convert JSON → JobItem
                var job = System.Text.Json.JsonSerializer.Deserialize<JobItem>(json);

                if (job != null)
                    await _queue.EnqueueAsync(job);
            }
        }
    }
}
