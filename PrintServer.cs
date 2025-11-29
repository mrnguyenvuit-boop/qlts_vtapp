using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ClientPrinterTray
{
    public class PrintServer
    {
        private readonly PrintQueue _queue;
        private readonly AppSettings _settings;
        private HttpListener? _listener;
        private bool _running = false;

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
            _listener.Prefixes.Add(url); // ▶ BẮT BUỘC có dấu "/" cuối
            _listener.Start();

            Console.WriteLine("🔥 WS PrintServer listening at: " + url);

            _ = Task.Run(async () =>
            {
                while (_running)
                {
                    var ctx = await _listener.GetContextAsync();
                    if (ctx.Request.IsWebSocketRequest)
                        _ = HandleWebSocket(ctx);
                    else
                        ctx.Response.Close();
                }
            });
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }

        private async Task HandleWebSocket(HttpListenerContext ctx)
        {
            var ws = (await ctx.AcceptWebSocketAsync(null)).WebSocket;
            Console.WriteLine("🟢 WebSocket connected from Web!");
            _queue.Socket = ws;  // ⭐ GÁN ĐỂ QUEUE CÓ THỂ GỬI NGƯỢC VỀ WEB

            byte[] buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.CloseStatus.HasValue) break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("📥 Incoming JSON: " + json);

                try
                {
                    var req = JsonSerializer.Deserialize<PrintRequest>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (req?.Files != null && req.Files.Count > 0)
                    {
                        Console.WriteLine($"🖨 Queueing {req.Files.Count} files to print...");
                        await _queue.EnqueueAsync(req.Files);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❗ JSON Parse ERROR: " + ex.Message);
                }
            }

            Console.WriteLine("🔻 WebSocket closed.");
        }
    }
        public class PrintRequest
        {
            // ⭐ Đây là property JSON duy nhất để nhận files từ web
            [JsonPropertyName("files")]
            public List<string> Files { get; set; } = new();

            // Nếu muốn sau này hỗ trợ thêm key khác → chỉ cho phép ghi vào Files
            [JsonPropertyName("docs")]
            public List<string>? Docs { set => Merge(value); }

            [JsonPropertyName("file")]
            public string? FileSingle { set => Merge(new List<string> { value! }); }

            [JsonPropertyName("base64")]
            public string? Base64Pdf { set => Merge(new List<string> { "base64:" + value }); }

            private void Merge(List<string>? list)
            {
                if (list == null) return;
                Files.AddRange(list.Where(x => !string.IsNullOrEmpty(x)));
            }
        }
    
}