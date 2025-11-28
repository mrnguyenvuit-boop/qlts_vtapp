namespace ClientPrinterTray
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ComboBox cboPrinters;
        private System.Windows.Forms.Button btnSetDefault;
        private System.Windows.Forms.Button btnRefreshPrinters;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnTestPrint;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label labelPrinter;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.DataGridView dgvJobs;

        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.Panel controlPanel;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            refreshTimer = new System.Windows.Forms.Timer(components);
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
            dgvJobs = new DataGridView();
            controlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJobs).BeginInit();
            SuspendLayout();
            // 
            // refreshTimer
            // 
            refreshTimer.Interval = 2000;
            // 
            // controlPanel
            // 
            controlPanel.BackColor = Color.WhiteSmoke;
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
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Location = new Point(0, 0);
            controlPanel.Name = "controlPanel";
            controlPanel.Size = new Size(884, 100);
            controlPanel.TabIndex = 1;
            // 
            // labelPrinter
            // 
            labelPrinter.Location = new Point(18, 17);
            labelPrinter.Name = "labelPrinter";
            labelPrinter.Size = new Size(57, 23);
            labelPrinter.TabIndex = 0;
            labelPrinter.Text = "Máy in:";
            // 
            // cboPrinters
            // 
            cboPrinters.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPrinters.Location = new Point(75, 14);
            cboPrinters.Name = "cboPrinters";
            cboPrinters.Size = new Size(300, 23);
            cboPrinters.TabIndex = 1;
            // 
            // btnRefreshPrinters
            // 
            btnRefreshPrinters.BackColor = Color.LightSteelBlue;
            btnRefreshPrinters.Location = new Point(380, 12);
            btnRefreshPrinters.Name = "btnRefreshPrinters";
            btnRefreshPrinters.Size = new Size(80, 23);
            btnRefreshPrinters.TabIndex = 2;
            btnRefreshPrinters.Text = "Reload 🔄";
            btnRefreshPrinters.UseVisualStyleBackColor = false;
            // 
            // btnSetDefault
            // 
            btnSetDefault.BackColor = Color.LightGreen;
            btnSetDefault.Location = new Point(465, 12);
            btnSetDefault.Name = "btnSetDefault";
            btnSetDefault.Size = new Size(120, 23);
            btnSetDefault.TabIndex = 3;
            btnSetDefault.Text = "Đặt mặc định ⭐";
            btnSetDefault.UseVisualStyleBackColor = false;
            // 
            // labelPort
            // 
            labelPort.Location = new Point(18, 50);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(35, 23);
            labelPort.TabIndex = 4;
            labelPort.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(75, 49);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(79, 23);
            txtPort.TabIndex = 5;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.LightGreen;
            btnStart.Location = new Point(160, 50);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(80, 23);
            btnStart.TabIndex = 6;
            btnStart.Text = "▶ Start";
            btnStart.UseVisualStyleBackColor = false;
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.LightCoral;
            btnStop.Location = new Point(250, 50);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(80, 23);
            btnStop.TabIndex = 7;
            btnStop.Text = "■ Stop";
            btnStop.UseVisualStyleBackColor = false;
            // 
            // btnTestPrint
            // 
            btnTestPrint.BackColor = Color.SkyBlue;
            btnTestPrint.Location = new Point(350, 50);
            btnTestPrint.Name = "btnTestPrint";
            btnTestPrint.Size = new Size(100, 23);
            btnTestPrint.TabIndex = 8;
            btnTestPrint.Text = "Test Print 📄";
            btnTestPrint.UseVisualStyleBackColor = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(470, 55);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(100, 15);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "Status: STOPPED";
            // 
            // dgvJobs
            // 
            dgvJobs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvJobs.BackgroundColor = Color.White;
            dgvJobs.Location = new Point(0, 106);
            dgvJobs.Name = "dgvJobs";
            dgvJobs.ReadOnly = true;
            dgvJobs.Size = new Size(878, 441);
            dgvJobs.TabIndex = 0;
            // 
            // MainForm
            // 
            ClientSize = new Size(884, 561);
            Controls.Add(dgvJobs);
            Controls.Add(controlPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hệ thống hỗ trợ In PDF";
            Load += MainForm_Load;
            controlPanel.ResumeLayout(false);
            controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJobs).EndInit();
            ResumeLayout(false);
        }
    }
}
