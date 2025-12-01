using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;

// EPPlus (Excel Export)
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ClientPrinterTray
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly JobStore _store;
        private readonly PrintQueue _queue;
        private readonly PrintServer _server;

        // Phân trang log
        private int pageSize = 50;
        private int currentPage = 1;
        private int totalPages = 1;

        // NotifyIcon
        private NotifyIcon tray;
        private ContextMenuStrip trayMenu;

        public MainForm(AppSettings settings, JobStore store, PrintQueue queue, PrintServer server)
        {
            _settings = settings;
            _store = store;
            _queue = queue;
            _server = server;

            InitializeComponent();

            // ================== FORM LOAD ==================
            this.Load += MainForm_Load;

            // 🔥 Refresh nhẹ - tránh giật khi click
            refreshTimer.Tick += (_, __) =>
            {
                if (!dgvJobs.Focused && !dgvJobs.IsCurrentCellInEditMode)
                    BindJobs();
            };

            // ================== BUTTON ==================
            btnRefreshPrinters.Click += (_, __) => LoadPrinters();
            btnSetDefault.Click += btnSetDefault_Click;
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnTestPrint.Click += btnTestPrint_Click;

            // ================== FILTER + EXPORT ==================
            btnFilter.Click += (_, __) => FilterLog();
            btnClear.Click += (_, __) => ClearLogs();
            btnExportExcel.Click += (_, __) => ExportExcel();

            // ================== QUEUE CALLBACK (NÊN GIỮ) ==================
            _queue.JobCompleted += async _ => BeginInvoke((Action)(() => BindJobs()));
            _queue.PrintFinished += async _ => BeginInvoke((Action)(() => BindJobs()));

            LoadPrinters();
            CleanOldLogs();
            BindJobs();
            refreshTimer.Start();
        }


        //================ MAIN LOAD ==================
        private void MainForm_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            EnableAutoStart(true);
            SetupTrayIcon();
            txtPort.Text = _settings.Port.ToString();

            // text footer ban đầu
            if (statusText != null)
                statusText.Text = "🔸 Ready...";
        }

        //================ AUTOSTART ==================
        public void EnableAutoStart(bool enable)
        {
            var k = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (enable)
                k.SetValue("ClientPrinterTray", Application.ExecutablePath);
            else
                k.DeleteValue("ClientPrinterTray", false);
        }

        //================ START/STOP ==================
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cboPrinters.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn máy in trước khi Start!");
                return;
            }

            if (int.TryParse(txtPort.Text, out var port))
            {
                _settings.Port = port;
                _settings.DefaultPrinter = cboPrinters.SelectedItem.ToString();
                _settings.Save();
            }

            _server.Start();
            lblStatus.Text = $"Status: RUNNING → ws://localhost:{_settings.Port}/ws";
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            if (statusText != null)
            {
                statusText.Text = $"🟢 RUNNING - Port {_settings.Port}";
                statusText.ForeColor = Color.Green;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _server.Stop();
            lblStatus.Text = "Status: STOPPED";

            btnStart.Enabled = true;
            btnStop.Enabled = false;

            if (statusText != null)
            {
                statusText.Text = "🟥 STOPPED";
                statusText.ForeColor = Color.Red;
            }
        }

        //================ PRINTER LIST ==================
        private void LoadPrinters()
        {
            cboPrinters.Items.Clear();

            foreach (var p in Printer.GetPrinters())
                cboPrinters.Items.Add(p);

            // Nếu có lưu máy in cũ → thử gán lại
            if (!string.IsNullOrEmpty(_settings.DefaultPrinter))
            {
                int i = cboPrinters.FindStringExact(_settings.DefaultPrinter);
                if (i >= 0)
                {
                    cboPrinters.SelectedIndex = i;
                    return; // đã chọn được → thoát
                }
            }

            // Fallback - nếu không tìm ra máy in cũ thì chọn máy đầu tiên
            if (cboPrinters.Items.Count > 0)
                cboPrinters.SelectedIndex = 0;
        }

        private void btnSetDefault_Click(object sender, EventArgs e)
        {
            if (cboPrinters.SelectedItem != null)
            {
                _settings.DefaultPrinter = cboPrinters.SelectedItem.ToString();
                _settings.Save();
                MessageBox.Show("Đã đặt máy in mặc định cho ứng dụng!");
            }
            else
            {
                MessageBox.Show("Chưa chọn máy in.");
            }
        }

        //================ TEST PRINT ==================
        private async void btnTestPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_settings.DefaultPrinter))
            {
                MessageBox.Show("Vui lòng chọn và lưu máy in mặc định trước khi test in.");
                return;
            }

            var pdf = Path.Combine(Path.GetTempPath(), "test_print.pdf");
            File.WriteAllBytes(pdf, Convert.FromBase64String(TEST_PDF_BASE64));

            await  Printer.PrintSilentAsync(pdf, _settings.DefaultPrinter);
            MessageBox.Show("Đã gửi lệnh in Test PDF!");
        }

        private const string TEST_PDF_BASE64 =
"JVBERi0xLjMKMSAwIG9iago8PC9UeXBlIC9DYXRhbG9nL1BhZ2VzIDIgMCBSPj4KZW5kb2JqCjIgMCBvYmoKPDwvVHlwZSAvUGFnZXMvS2lkcyBbMyAwIFJdL0NvdW50IDE+PgplbmRvYmoKMyAwIG9iago8PC9UeXBlIC9QYWdlL1BhcmVudCAyIDAgUi9NZWRpYUJveCBbMCAwIDYxMiA3OTJdL0NvbnRlbnRzIDQgMCBSPj4KZW5kb2JqCjQgMCBvYmoKPDwvTGVuZ3RoIDU5Pj4Kc3RyZWFtCkJUIApUICBUZXN0IFByaW50IFBERiAhIQplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA1CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDUzIDAwMDAwIG4gCjAwMDAwMDAxMjAgMDAwMDAgbiAKMDAwMDAwMDIwMCAwMDAwMCBuIAp0cmFpbGVyCjw8L1Jvb3QgMSAwIFIvU2l6ZSA1Pj4Kc3RhcnR4cmVmCjI1NAolJUVPRgo=";

        //================ BIND GRID (CÓ PHÂN TRANG) ==================
        private void BindJobs(int page = -1)
        {
            var all = _store.GetAll().OrderByDescending(x => x.Created).ToList();
            int totalRows = all.Count;

            if (totalRows == 0)
            {
                dgvJobs.DataSource = null;
                currentPage = 1;
                totalPages = 1;
                UpdatePagingFooter(0);
                return;
            }

            // cập nhật currentPage nếu có truyền tham số
            if (page > 0) currentPage = page;

            // tính lại totalPages và clamp currentPage
            totalPages = (int)Math.Ceiling(totalRows / (double)pageSize);
            if (currentPage > totalPages) currentPage = totalPages;
            if (currentPage < 1) currentPage = 1;

            // Lưu JobId đang chọn để giữ selection
            string? selectedId = null;
            if (dgvJobs.CurrentRow != null &&
                dgvJobs.CurrentRow.Cells["JobId"] != null)
            {
                selectedId = dgvJobs.CurrentRow.Cells["JobId"].Value?.ToString();
            }

            var data = all
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
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

            dgvJobs.DataSource = data;

            // Khôi phục selection nếu có
            if (!string.IsNullOrEmpty(selectedId))
            {
                foreach (DataGridViewRow row in dgvJobs.Rows)
                {
                    if (row.Cells["JobId"].Value?.ToString() == selectedId)
                    {
                        row.Selected = true;
                        dgvJobs.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }

            UpdatePagingFooter(totalRows);
        }

        // Hiển thị thông tin phân trang ở footer
        private void UpdatePagingFooter(int totalRows)
        {
            if (statusText == null) return;

            if (totalRows == 0)
            {
                statusText.Text = "Không có log nào.";
            }
            else
            {
                statusText.Text = $"Trang {currentPage}/{totalPages} - Tổng {totalRows} dòng log";
            }
        }

        //================ FILTER LOG ==================
        private void FilterLog()
        {
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date;

            var list = _store.GetAll()
                .Where(x => x.Created.Date >= from && x.Created.Date <= to)
                .OrderByDescending(x => x.Created)
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

            if (statusText != null)
                statusText.Text = $"Lọc từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy} - {list.Count} dòng.";
        }

        //================ CLEAR LOG ==================
        private void ClearLogs()
        {
            if (MessageBox.Show("Bạn chắc chắn muốn xóa toàn bộ nhật ký?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _store.Clear();
                currentPage = 1;
                BindJobs();

                if (statusText != null)
                    statusText.Text = "Đã xóa toàn bộ log.";
            }
        }

        //================ CLEAN OLD LOGS (>30 days) ==================
        private void CleanOldLogs()
        {
            var cutoff = DateTime.Now.AddDays(-30);
            var keep = _store.GetAll().Where(x => x.Created >= cutoff).ToList();
            _store.SaveAll(keep);
        }

        //================ EXPORT EXCEL ==================
        private void ExportExcel()
        {
            var list = _store.GetAll();
            if (!list.Any())
            {
                MessageBox.Show("Không có dữ liệu log để xuất Excel.");
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Chọn nơi lưu";
                sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                sfd.FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var pkg = new ExcelPackage())
                    {
                        var ws = pkg.Workbook.Worksheets.Add("LOG");
                        ws.Cells["A1"].LoadFromCollection(list, true);
                        ws.Cells.AutoFitColumns();
                        pkg.SaveAs(new FileInfo(sfd.FileName));
                    }

                    MessageBox.Show($"✔ Xuất Excel thành công!\n📁 {sfd.FileName}",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //================ TRAY ICON ==================
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

        //================ FORM BEHAVIOR ==================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
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
                Hide();
            }
        }

        // Event trống nếu Designer còn gắn
        private void dtTo_ValueChanged(object sender, EventArgs e) { }
        private void btnClear_Click(object sender, EventArgs e) { }
        private void dtFrom_ValueChanged(object sender, EventArgs e) { }
    }
}
