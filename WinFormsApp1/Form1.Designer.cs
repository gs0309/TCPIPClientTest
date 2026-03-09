namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnStart = new Button();
            txtDuration = new TextBox();
            label1 = new Label();
            label2 = new Label();
            lblCountdown = new Label();
            rtbDataDisplay = new RichTextBox();
            label3 = new Label();
            label4 = new Label();
            lblStartTime = new Label();
            lblEndTime = new Label();
            label5 = new Label();
            lblDataCount = new Label();
            timerCountdown = new System.Windows.Forms.Timer(components);
            timerRealTime = new System.Windows.Forms.Timer(components);
            btnParse = new Button();
            dataGridView1 = new DataGridView();
            progressBarParse = new ProgressBar();
            btnExportCsv = new Button();
            btnExportTxt = new Button();
            btnRealTimeMonitor = new Button();
            rtbRealTimeDisplay = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(267, 23);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 40);
            btnStart.TabIndex = 0;
            btnStart.Text = "log数据";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // txtDuration
            // 
            txtDuration.Location = new Point(373, 32);
            txtDuration.Name = "txtDuration";
            txtDuration.Size = new Size(50, 23);
            txtDuration.TabIndex = 1;
            txtDuration.Text = "1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(443, 35);
            label1.Name = "label1";
            label1.Size = new Size(20, 17);
            label1.TabIndex = 2;
            label1.Text = "秒";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(485, 34);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 3;
            label2.Text = "倒计时：";
            // 
            // lblCountdown
            // 
            lblCountdown.AutoSize = true;
            lblCountdown.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblCountdown.Location = new Point(547, 31);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(20, 22);
            lblCountdown.TabIndex = 4;
            lblCountdown.Text = "0";
            // 
            // rtbDataDisplay
            // 
            rtbDataDisplay.Location = new Point(12, 80);
            rtbDataDisplay.Name = "rtbDataDisplay";
            rtbDataDisplay.Size = new Size(776, 250);
            rtbDataDisplay.TabIndex = 5;
            rtbDataDisplay.Text = "";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 464);
            label3.Name = "label3";
            label3.Size = new Size(68, 17);
            label3.TabIndex = 6;
            label3.Text = "开始时间：";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 484);
            label4.Name = "label4";
            label4.Size = new Size(68, 17);
            label4.TabIndex = 7;
            label4.Text = "结束时间：";
            // 
            // lblStartTime
            // 
            lblStartTime.AutoSize = true;
            lblStartTime.Location = new Point(98, 464);
            lblStartTime.Name = "lblStartTime";
            lblStartTime.Size = new Size(0, 17);
            lblStartTime.TabIndex = 8;
            // 
            // lblEndTime
            // 
            lblEndTime.AutoSize = true;
            lblEndTime.Location = new Point(98, 484);
            lblEndTime.Name = "lblEndTime";
            lblEndTime.Size = new Size(0, 17);
            lblEndTime.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 504);
            label5.Name = "label5";
            label5.Size = new Size(80, 17);
            label5.TabIndex = 10;
            label5.Text = "接收数据量：";
            // 
            // lblDataCount
            // 
            lblDataCount.AutoSize = true;
            lblDataCount.Location = new Point(98, 504);
            lblDataCount.Name = "lblDataCount";
            lblDataCount.Size = new Size(0, 17);
            lblDataCount.TabIndex = 11;
            // 
            // timerCountdown
            // 
            timerCountdown.Interval = 1000;
            timerCountdown.Tick += timerCountdown_Tick;
            // 
            // timerRealTime
            // 
            timerRealTime.Tick += timerRealTime_Tick;
            // 
            // btnParse
            // 
            btnParse.Location = new Point(408, 460);
            btnParse.Name = "btnParse";
            btnParse.Size = new Size(100, 40);
            btnParse.TabIndex = 12;
            btnParse.Text = "解析数据";
            btnParse.UseVisualStyleBackColor = true;
            btnParse.Click += btnParse_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 538);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(776, 72);
            dataGridView1.TabIndex = 13;
            // 
            // progressBarParse
            // 
            progressBarParse.Location = new Point(408, 508);
            progressBarParse.Name = "progressBarParse";
            progressBarParse.Size = new Size(328, 20);
            progressBarParse.TabIndex = 13;
            progressBarParse.Visible = false;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Location = new Point(570, 460);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(100, 40);
            btnExportCsv.TabIndex = 15;
            btnExportCsv.Text = "导出CSV";
            btnExportCsv.UseVisualStyleBackColor = true;
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // btnExportTxt
            // 
            btnExportTxt.Location = new Point(624, 23);
            btnExportTxt.Name = "btnExportTxt";
            btnExportTxt.Size = new Size(100, 40);
            btnExportTxt.TabIndex = 18;
            btnExportTxt.Text = "导出TXT";
            btnExportTxt.UseVisualStyleBackColor = true;
            btnExportTxt.Click += btnExportTxt_Click;
            // 
            // btnRealTimeMonitor
            // 
            btnRealTimeMonitor.Location = new Point(26, 14);
            btnRealTimeMonitor.Name = "btnRealTimeMonitor";
            btnRealTimeMonitor.Size = new Size(100, 40);
            btnRealTimeMonitor.TabIndex = 16;
            btnRealTimeMonitor.Text = "实时监控";
            btnRealTimeMonitor.UseVisualStyleBackColor = true;
            btnRealTimeMonitor.Click += btnRealTimeMonitor_Click;
            // 
            // rtbRealTimeDisplay
            // 
            rtbRealTimeDisplay.Location = new Point(12, 80);
            rtbRealTimeDisplay.Name = "rtbRealTimeDisplay";
            rtbRealTimeDisplay.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbRealTimeDisplay.Size = new Size(776, 348);
            rtbRealTimeDisplay.TabIndex = 17;
            rtbRealTimeDisplay.Text = "";
            rtbRealTimeDisplay.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 650);
            Controls.Add(dataGridView1);
            Controls.Add(btnParse);
            Controls.Add(btnExportCsv);
            Controls.Add(btnExportTxt);
            Controls.Add(btnRealTimeMonitor);
            Controls.Add(rtbRealTimeDisplay);
            Controls.Add(progressBarParse);
            Controls.Add(btnStart);
            Controls.Add(txtDuration);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(lblCountdown);
            Controls.Add(rtbDataDisplay);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(lblStartTime);
            Controls.Add(lblEndTime);
            Controls.Add(label5);
            Controls.Add(lblDataCount);
            Name = "Form1";
            Text = "Modbus TCP 数据接收";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnExportTxt;
        private System.Windows.Forms.TextBox txtDuration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.RichTextBox rtbDataDisplay;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblDataCount;
        private System.Windows.Forms.Timer timerCountdown;
        private System.Windows.Forms.Timer timerRealTime;
        private System.Windows.Forms.ProgressBar progressBarParse;
        private System.Windows.Forms.Button btnRealTimeMonitor;
        private System.Windows.Forms.RichTextBox rtbRealTimeDisplay;
    }
}
