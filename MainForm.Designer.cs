using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;

// EPPlus (Excel Export)
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ClientPrinterTray
{
    public partial class MainForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        private ComboBox cboPrinters;
        private Button btnSetDefault;
        private Button btnRefreshPrinters;
        private Button btnStart;
        private Button btnStop;
        private Button btnTestPrint;
        private TextBox txtPort;
        private Label lblStatus;
        private Label labelPrinter;
        private Label labelPort;
        private DataGridView dgvJobs;
        private System.Windows.Forms.Timer refreshTimer;
        private Panel controlPanel;

        // extra controls
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private Button btnFilter;
        private Button btnClear;
        private Button btnExportExcel;

        // 🔥 FOOTER STATUS BAR
        private StatusStrip statusBar;
        private ToolStripStatusLabel statusText;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // ====================== DESIGN UI ===========================
        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            controlPanel = new Panel();
            labelPrinter = new Label();
            cboPrinters = new ComboBox();
            btnRefreshPrinters = new Button();
            btnSetDefault = new Button();
            labelPort = new Label();
            txtPort = new TextBox();
            btnStart = new Button();
            btnStop = new Button();
            btnTestPrint = new Button();
            lblStatus = new Label();
            dtFrom = new DateTimePicker();
            dtTo = new DateTimePicker();
            btnFilter = new Button();
            btnClear = new Button();
            btnExportExcel = new Button();
            dgvJobs = new DataGridView();
            refreshTimer = new System.Windows.Forms.Timer(components);
            statusBar = new StatusStrip();
            statusText = new ToolStripStatusLabel();
            controlPanel.SuspendLayout();
            ((ISupportInitialize)dgvJobs).BeginInit();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // controlPanel
            // 
            controlPanel.Controls.Add(labelPrinter);
            controlPanel.Controls.Add(cboPrinters);
            controlPanel.Controls.Add(btnRefreshPrinters);
            controlPanel.Controls.Add(btnSetDefault);
            controlPanel.Controls.Add(labelPort);
            controlPanel.Controls.Add(txtPort);
            controlPanel.Controls.Add(btnStart);
            controlPanel.Controls.Add(btnStop);
            controlPanel.Controls.Add(btnTestPrint);
            controlPanel.Controls.Add(lblStatus);
            controlPanel.Controls.Add(dtFrom);
            controlPanel.Controls.Add(dtTo);
            controlPanel.Controls.Add(btnFilter);
            controlPanel.Controls.Add(btnClear);
            controlPanel.Controls.Add(btnExportExcel);
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Location = new Point(0, 0);
            controlPanel.Name = "controlPanel";
            controlPanel.Padding = new Padding(10);
            controlPanel.Size = new Size(900, 120);
            controlPanel.TabIndex = 1;
            // 
            // labelPrinter
            // 
            labelPrinter.Location = new Point(13, 12);
            labelPrinter.Name = "labelPrinter";
            labelPrinter.Size = new Size(62, 23);
            labelPrinter.TabIndex = 0;
            labelPrinter.Text = "Máy in:";
            // 
            // cboPrinters
            // 
            cboPrinters.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPrinters.Location = new Point(81, 10);
            cboPrinters.Name = "cboPrinters";
            cboPrinters.Size = new Size(200, 23);
            cboPrinters.TabIndex = 1;
            // 
            // btnRefreshPrinters
            // 
            btnRefreshPrinters.Location = new Point(291, 10);
            btnRefreshPrinters.Name = "btnRefreshPrinters";
            btnRefreshPrinters.Size = new Size(75, 23);
            btnRefreshPrinters.TabIndex = 2;
            btnRefreshPrinters.Text = "Tải lại";
            // 
            // btnSetDefault
            // 
            btnSetDefault.Location = new Point(366, 10);
            btnSetDefault.Name = "btnSetDefault";
            btnSetDefault.Size = new Size(75, 23);
            btnSetDefault.TabIndex = 3;
            btnSetDefault.Text = "Lưu máy in";
            // 
            // labelPort
            // 
            labelPort.Location = new Point(13, 47);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(62, 23);
            labelPort.TabIndex = 4;
            labelPort.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(81, 45);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(60, 23);
            txtPort.TabIndex = 5;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(151, 45);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 6;
            btnStart.Text = "Start";
            // 
            // btnStop
            // 
            btnStop.Location = new Point(232, 45);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 23);
            btnStop.TabIndex = 7;
            btnStop.Text = "Stop";
            // 
            // btnTestPrint
            // 
            btnTestPrint.Location = new Point(313, 45);
            btnTestPrint.Name = "btnTestPrint";
            btnTestPrint.Size = new Size(75, 23);
            btnTestPrint.TabIndex = 8;
            btnTestPrint.Text = "Test in PDF";
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(81, 87);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(300, 23);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "Status: STOPPED";
            // 
            // dtFrom
            // 
            dtFrom.Location = new Point(544, 8);
            dtFrom.Name = "dtFrom";
            dtFrom.Size = new Size(216, 23);
            dtFrom.TabIndex = 10;
            // 
            // dtTo
            // 
            dtTo.Location = new Point(544, 37);
            dtTo.Name = "dtTo";
            dtTo.Size = new Size(216, 23);
            dtTo.TabIndex = 11;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(781, 8);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(75, 23);
            btnFilter.TabIndex = 12;
            btnFilter.Text = "Lọc";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(781, 37);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 13;
            btnClear.Text = "Xóa lọc";
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(612, 66);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(75, 23);
            btnExportExcel.TabIndex = 14;
            btnExportExcel.Text = "Export Excel";
            // 
            // dgvJobs
            // 
            dgvJobs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvJobs.Dock = DockStyle.Fill;
            dgvJobs.Location = new Point(0, 120);
            dgvJobs.Name = "dgvJobs";
            dgvJobs.ReadOnly = true;
            dgvJobs.Size = new Size(900, 408);
            dgvJobs.TabIndex = 0;
            // 
            // statusBar
            // 
            statusBar.Items.AddRange(new ToolStripItem[] { statusText });
            statusBar.Location = new Point(0, 528);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(900, 22);
            statusBar.TabIndex = 2;
            // 
            // statusText
            // 
            statusText.ForeColor = Color.DimGray;
            statusText.Name = "statusText";
            statusText.Size = new Size(885, 17);
            statusText.Spring = true;
            statusText.Text = "🔸 Ready...";
            statusText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            ClientSize = new Size(900, 550);
            Controls.Add(dgvJobs);
            Controls.Add(controlPanel);
            Controls.Add(statusBar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 550);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hệ thống hỗ trợ In file - Trung Tâm GDTS Đà Nẵng";
            controlPanel.ResumeLayout(false);
            controlPanel.PerformLayout();
            ((ISupportInitialize)dgvJobs).EndInit();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // Update footer from code:
        public void SetStatus(string text, Color? color = null)
        {
            statusText.Text = text;
            if (color != null) statusText.ForeColor = color.Value;
        }
    }
}
