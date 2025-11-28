using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;

namespace ClientPrinterTray
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly JobStore _store;
        private readonly PrintQueue _queue;
        private readonly PrintServer _server;

        // chỉ dùng 1 NotifyIcon
        private NotifyIcon tray;
        private ContextMenuStrip trayMenu;

        public MainForm(AppSettings settings, JobStore store, PrintQueue queue, PrintServer server)
        {
            InitializeComponent();

            _settings = settings;
            _store = store;
            _queue = queue;
            _server = server;

            // 🔥 đảm bảo MainForm_Load được gọi
            this.Load += MainForm_Load;

            refreshTimer.Tick += (_, __) => BindJobs();

            // Gán event UI
            btnRefreshPrinters.Click += (_, __) => LoadPrinters();
            btnSetDefault.Click += btnSetDefault_Click;
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnTestPrint.Click += btnTestPrint_Click;

            // queue cập nhật realtime UI
            _queue.JobUpdated += _ => BeginInvoke(new Action(BindJobs));
        }

        // ================= MAIN LOAD =================
        private void MainForm_Load(object sender, EventArgs e)
        {
            // tự chạy cùng Windows
            EnableAutoStart(true);

            SetupTrayIcon();
            LoadPrinters();

            txtPort.Text = _settings.Port.ToString();
            BindJobs();
            refreshTimer.Start();
        }

        // ================= AUTOSTART =================
        public void EnableAutoStart(bool enable)
        {
            var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (enable)
                key.SetValue("ClientPrinterTray", Application.ExecutablePath);
            else
                key.DeleteValue("ClientPrinterTray", false);
        }

        // ================= SERVER START/STOP =================
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtPort.Text, out var port))
            {
                _settings.Port = port;
                _settings.Save();
            }

            _server.Start();
            lblStatus.Text = $"RUNNING → ws://localhost:{_settings.Port}/ws";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _server.Stop();
            lblStatus.Text = "STOPPED";
        }

        // ================= PRINTER LIST =================
        private void LoadPrinters()
        {
            cboPrinters.Items.Clear();
            foreach (var p in Printer.GetPrinters())
                cboPrinters.Items.Add(p);

            if (!string.IsNullOrEmpty(_settings.DefaultPrinter))
                cboPrinters.SelectedItem = _settings.DefaultPrinter;
        }

        private void btnSetDefault_Click(object sender, EventArgs e)
        {
            if (cboPrinters.SelectedItem != null)
            {
                _settings.DefaultPrinter = cboPrinters.SelectedItem.ToString();
                _settings.Save();
                MessageBox.Show("Đã đặt máy in mặc định!");
            }
        }

        // ================= TEST PRINT =================
        private void btnTestPrint_Click(object sender, EventArgs e)
        {
            var pdf = Path.Combine(Path.GetTempPath(), "test_print.pdf");
            File.WriteAllBytes(pdf, Convert.FromBase64String(TEST_PDF_BASE64));

            Printer.PrintSilent(pdf, _settings.DefaultPrinter);
            MessageBox.Show("Đã gửi lệnh in Test PDF!");
        }

        private const string TEST_PDF_BASE64 =
"JVBERi0xLjMKMSAwIG9iago8PC9UeXBlIC9DYXRhbG9nL1BhZ2VzIDIgMCBSPj4KZW5kb2JqCjIgMCBvYmoKPDwvVHlwZSAvUGFnZXMvS2lkcyBbMyAwIFJdL0NvdW50IDE+PgplbmRvYmoKMyAwIG9iago8PC9UeXBlIC9QYWdlL1BhcmVudCAyIDAgUi9NZWRpYUJveCBbMCAwIDYxMiA3OTJdL0NvbnRlbnRzIDQgMCBSPj4KZW5kb2JqCjQgMCBvYmoKPDwvTGVuZ3RoIDU5Pj4Kc3RyZWFtCkJUIApUICBUZXN0IFByaW50IFBERiAhIQplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA1CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDUzIDAwMDAwIG4gCjAwMDAwMDAxMjAgMDAwMDAgbiAKMDAwMDAwMDIwMCAwMDAwMCBuIAp0cmFpbGVyCjw8L1Jvb3QgMSAwIFIvU2l6ZSA1Pj4Kc3RhcnR4cmVmCjI1NAolJUVPRgo=";

        // ================= BIND GRID =================
        private void BindJobs()
        {
            var list = _store.GetAll()
                .Select(j => new
                {
                    j.JobId,
                    j.Printer,
                    State = j.State.ToString(),
                    Created = j.Created.ToString("yyyy-MM-dd HH:mm:ss"),
                    Completed = j.Completed?.ToString("yyyy-MM-dd HH:mm:ss"),
                    j.Error
                })
                .ToList();

            dgvJobs.DataSource = list;
        }

        // ================= TRAY ICON =================
        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Hiện cửa sổ", null, (_, __) => RestoreForm());
            trayMenu.Items.Add("Thoát", null, (_, __) =>
            {
                tray.Visible = false;
                Application.Exit();
            });

            tray = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                ContextMenuStrip = trayMenu,
                Text = "Client Printer Tray"
            };

            tray.DoubleClick += (_, __) => RestoreForm();
        }

        private void RestoreForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        // ================= FORM BEHAVIOR =================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide(); // Ẩn xuống tray khi nhấn X
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide(); // Ẩn xuống tray khi minimize
            }
        }
    }
}
