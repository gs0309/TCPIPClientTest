using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OfficeOpenXml;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private TcpClient? modbusClient;
        private NetworkStream? networkStream;
        private TcpClient? realTimeClient;
        private NetworkStream? realTimeStream;
        private int countdownSeconds;
        private DateTime startTime;
        private DateTime endTime;
        private bool isReceivingData;
        private bool isRealTimeMonitoring;
        private int totalBytesReceived;
        private List<byte[]> receivedDataList = new List<byte[]>();
        private object dataLock = new object();
        private List<byte> bufferAccumulator = new List<byte>();
        private byte[] latestFrameData = new byte[0];
        private List<ParseDataRow> parsedDataList = new List<ParseDataRow>();
        private const int FRAME_SIZE = 2006; // 每帧2006字节（增加了2字节帧计数）

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isReceivingData)
            {
                StartDataReception();
            }
        }

        private void btnRealTimeMonitor_Click(object sender, EventArgs e)
        {
            if (!isRealTimeMonitoring)
            {
                StartRealTimeMonitoring();
            }
            else
            {
                StopRealTimeMonitoring();
            }
        }

        private void StartDataReception()
        {
            try
            {
                // Parse duration
                if (!int.TryParse(txtDuration.Text, out countdownSeconds) || countdownSeconds <= 0)
                {
                    MessageBox.Show("请输入有效的接收时长（秒）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update UI
                btnStart.Enabled = false;
                btnParse.Enabled = false; // 禁用解析按钮
                //btnExport.Enabled = false; // 禁用导出按钮
                btnExportCsv.Enabled = false; // 禁用导出CSV按钮
                lblCountdown.Text = countdownSeconds.ToString();
                rtbDataDisplay.Clear();
                lblStartTime.Text = "";
                lblEndTime.Text = "";
                lblDataCount.Text = "";
                totalBytesReceived = 0;
                receivedDataList.Clear();
                bufferAccumulator.Clear();
                parsedDataList.Clear();

                // Record start time
                startTime = DateTime.Now;
                lblStartTime.Text = startTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // Start countdown timer
                timerCountdown.Start();

                // Start data reception in a separate thread
                isReceivingData = true;
                Thread receiveThread = new Thread(ReceiveModbusData);
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动接收失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
                btnParse.Enabled = true; // 启用解析按钮
                //btnExport.Enabled = true; // 启用导出按钮
                btnExportCsv.Enabled = true; // 启用导出CSV按钮
                isReceivingData = false;
            }
        }

        private void ReceiveModbusData()
        {
            try
            {
                // Connect to Modbus TCP server
                modbusClient = new TcpClient();
                modbusClient.ReceiveBufferSize = 65536;
                modbusClient.Connect("192.168.1.75", 504);
                networkStream = modbusClient.GetStream();
                networkStream.ReadTimeout = 100;

                byte[] buffer = new byte[65536];
                int bytesRead;

                // Receive data until countdown ends
                while (isReceivingData)
                {
                    try
                    {
                        // Check if data is available before reading
                        if (networkStream.DataAvailable)
                        {
                            // Read all available data
                            bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                               
                                // Add received data to accumulator and process frames
                                lock (dataLock)
                                {
                                    for (int i = 0; i < bytesRead; i++)
                                    {
                                        bufferAccumulator.Add(buffer[i]);
                                    }
                                    
                                    // Process complete frames from accumulator
                                    ProcessCompleteFrames();
                                }
                                
                                // Update data count display outside the lock to avoid deadlock
                               UpdateDataCountDisplay();
                            }
                        }
                        else
                        {
                            // Small delay to prevent CPU overload when no data
                            Thread.Sleep(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (isReceivingData)
                        {
                            UpdateDataDisplay("接收错误: " + ex.Message);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateDataDisplay("错误: " + ex.Message);
            }
            finally
            {
                // Clean up
                if (networkStream != null)
                    networkStream.Close();
                if (modbusClient != null)
                    modbusClient.Close();
            }
        }

        private void UpdateDataDisplay(string data)
        {
            if (rtbDataDisplay.InvokeRequired)
            {
                rtbDataDisplay.Invoke(new Action<string>(UpdateDataDisplay), data);
            }
            else
            {
                rtbDataDisplay.AppendText(data + Environment.NewLine);
                rtbDataDisplay.ScrollToCaret();
            }
        }

        private void UpdateDataCountDisplay()
        {
            if (lblDataCount.InvokeRequired)
            {
                lblDataCount.Invoke(new Action(UpdateDataCountDisplay));
            }
            else
            {
                lblDataCount.Text = totalBytesReceived + " 字节";
            }
        }

        private string ByteArrayToHexString(byte[] bytes, int length)
        {
            StringBuilder hexBuilder = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("X2") + " ");
            }
            return hexBuilder.ToString().Trim();
        }

        private void timerCountdown_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            lblCountdown.Text = countdownSeconds.ToString();

            if (countdownSeconds <= 0)
            {
                StopDataReception();
            }
        }

        private void timerRealTime_Tick(object sender, EventArgs e)
        {
            UpdateRealTimeDisplay();
        }

        private void StopDataReception()
        {
            // Stop timer
            timerCountdown.Stop();

            // Record end time
            endTime = DateTime.Now;
            lblEndTime.Text = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Stop data reception
            isReceivingData = false;

            // Give time for the receive thread to clean up
            Thread.Sleep(500);

            // Display all received data
            DisplayAllReceivedData();

            // Update UI
            btnStart.Enabled = true;
            btnParse.Enabled = true; // 启用解析按钮
            //btnExport.Enabled = true; // 启用导出按钮
            btnExportCsv.Enabled = true; // 启用导出CSV按钮
        }

        private void DisplayAllReceivedData()
        {
            if (rtbDataDisplay.InvokeRequired)
            {
                rtbDataDisplay.Invoke(new Action(DisplayAllReceivedData));
            }
            else
            {
                lock (dataLock)
                {
                    // Process any remaining data in accumulator
                    ProcessCompleteFrames();
                    
                    rtbDataDisplay.Clear();
                    rtbDataDisplay.AppendText($"总计接收：{totalBytesReceived} 字节" + Environment.NewLine);
                    rtbDataDisplay.AppendText($"帧数量：{receivedDataList.Count}" + Environment.NewLine);
                    if (receivedDataList.Count > 0)
                    {
                        double avgBytesPerFrame = (double)totalBytesReceived / receivedDataList.Count;
                        rtbDataDisplay.AppendText($"平均每帧：{avgBytesPerFrame:F2} 字节" + Environment.NewLine);
                        rtbDataDisplay.AppendText($"预期比例：{FRAME_SIZE} 字节/帧" + Environment.NewLine);
                    }
                    if (bufferAccumulator.Count > 0)
                    {
                        rtbDataDisplay.AppendText($"剩余未处理数据：{bufferAccumulator.Count} 字节" + Environment.NewLine);
                    }
                    
                    rtbDataDisplay.ScrollToCaret();
                }
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            // 在后台线程中执行解析，避免阻塞UI
            btnParse.Enabled = false;
            
            // 显示并初始化进度条
            if (progressBarParse.InvokeRequired)
            {
                progressBarParse.Invoke(new Action(() => {
                    progressBarParse.Visible = true;
                    progressBarParse.Minimum = 0;
                    progressBarParse.Maximum = 100;
                    progressBarParse.Value = 0;
                }));
            }
            else
            {
                progressBarParse.Visible = true;
                progressBarParse.Minimum = 0;
                progressBarParse.Maximum = 100;
                progressBarParse.Value = 0;
            }
            
            Thread parseThread = new Thread(new ThreadStart(ParseReceivedDataInBackground));
            parseThread.IsBackground = true;
            parseThread.Start();
        }

        private void ParseReceivedDataInBackground()
        {
            // 先在后台完成所有解析计算
            List<ParseDataRow> parseResults = new List<ParseDataRow>();
            
            lock (dataLock)
            {
                if (receivedDataList.Count == 0)
                {
                    UpdateParseResult(new List<ParseDataRow>(), true, "没有数据可解析，请先接收数据。");
                    return;
                }
                
                int totalFrames = receivedDataList.Count;
                int frameIndex = 0;
                foreach (byte[] frameData in receivedDataList)
                {
                    // 解析帧计数（低位在前，高位在后）
                    ushort frameCount = 0;
                    if (frameData.Length >= 6) // 4字节帧头 + 2字节帧计数
                    {
                        frameCount = (ushort)((frameData[5] << 8) | frameData[4]);
                    }
                    
                    // 解析50组数据
                    for (int group = 0; group < 50; group++)
                    {
                        // 计算每组数据的起始位置（跳过4字节帧头和2字节帧计数）
                        int groupStart = 4 + 2 + group * (24 + 16); // 4字节帧头 + 2字节帧计数 + 24字节模拟量 + 16字节编码器
                        
                        if (groupStart + 24 + 16 <= frameData.Length)
                        {
                            // 解析12通道模拟量数据
                            ushort[] analogValues = new ushort[12];//码值
                           
                            for (int channel = 0; channel < 12; channel++)
                            {
                                int analogStart = groupStart + channel * 2;
                                if (analogStart + 1 < frameData.Length)
                                {
                                    // 低位在前，高位在后
                                    analogValues[channel] = (ushort)((frameData[analogStart + 1] << 8) | frameData[analogStart]);
                                }
                            }
                            
                            // 解析4通道编码器数据
                            int[] encoderValues = new int[4];
                            short[] encoderHigh = new short[4];  // 高位为有符号数据
                            ushort[] encoderLow = new ushort[4];
                            
                            for (int encoder = 0; encoder < 4; encoder++)
                            {
                                int encoderStart = groupStart + 24 + encoder * 4;
                                if (encoderStart + 3 < frameData.Length)
                                {
                                    // 交换高位和低位，高位为有符号数据
                                    // 原始数据格式：[低字节0][低字节1][高字节0][高字节1]
                                    // 交换后：高位在前，低位在后
                                    encoderHigh[encoder] = (short)((frameData[encoderStart + 1] << 8) | frameData[encoderStart ]);
                                    encoderLow[encoder] = (ushort)((frameData[encoderStart + 3] << 8) | frameData[encoderStart+2]);
                                    encoderValues[encoder] = encoderHigh[encoder] * 60000 + encoderLow[encoder];
                                }
                            }
                            
                            // 创建解析数据行
                            ParseDataRow row = new ParseDataRow
                            {
                                FrameIndex = frameIndex + 1,
                                GroupIndex = group + 1,
                                FrameCount = frameCount,
                                AnalogValues = analogValues,
                                EncoderValues = encoderValues,
                                EncoderHigh = encoderHigh,
                                EncoderLow = encoderLow
                            };
                            
                            parseResults.Add(row);
                        }
                    }
                    
                    frameIndex++;
                    
                    // 更新进度条
                    int progress = (int)((float)frameIndex / totalFrames * 100);
                    if (progressBarParse.InvokeRequired)
                    {
                        progressBarParse.Invoke(new Action(() => {
                            progressBarParse.Value = progress;
                        }));
                    }
                    else
                    {
                        progressBarParse.Value = progress;
                    }
                    
                    // 短暂休眠，让UI有机会更新
                    Thread.Sleep(10);
                }
            }
            
            // 批量更新UI
            UpdateParseResult(parseResults, false, "");
        }

        private void UpdateParseResult(List<ParseDataRow> parseResults, bool isError, string errorMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<ParseDataRow>, bool, string>(UpdateParseResult), parseResults, isError, errorMessage);
            }
            else
            {
                if (isError)
                {
                    MessageBox.Show(errorMessage, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    parsedDataList = parseResults;
                    MessageBox.Show($"解析完成！共解析 {parseResults.Count} 条数据记录。", "解析完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                btnParse.Enabled = true;
                
                // 隐藏并重置进度条
                progressBarParse.Visible = false;
                progressBarParse.Value = 0;
            }
        }

        private void StartRealTimeMonitoring()
        {
            try
            {
                // 更新UI
                btnRealTimeMonitor.Text = "停止监控";
                btnStart.Enabled = false;
                rtbDataDisplay.Visible = false;
                rtbRealTimeDisplay.Visible = true;
                rtbRealTimeDisplay.Clear();
                rtbRealTimeDisplay.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular);

                // 开始实时监控
                isRealTimeMonitoring = true;
                latestFrameData = new byte[0];

                // 启动接收线程
                Thread receiveThread = new Thread(ReceiveRealTimeData);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                // 启动显示定时器
                timerRealTime.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动实时监控失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRealTimeMonitor.Text = "实时监控";
                btnStart.Enabled = true;
                isRealTimeMonitoring = false;
            }
        }

        private void StopRealTimeMonitoring()
        {
            // 停止定时器
            timerRealTime.Stop();

            // 停止接收
            isRealTimeMonitoring = false;

            // 清理连接
            if (realTimeStream != null)
            {
                try { realTimeStream.Close(); } catch { }
                realTimeStream = null;
            }
            if (realTimeClient != null)
            {
                try { realTimeClient.Close(); } catch { }
                realTimeClient = null;
            }

            // 更新UI
            btnRealTimeMonitor.Text = "实时监控";
            btnStart.Enabled = true;
            rtbRealTimeDisplay.AppendText("实时监控已停止" + Environment.NewLine);
        }

        private void ReceiveRealTimeData()
        {
            try
            {
                realTimeClient = new TcpClient();
                realTimeClient.ReceiveBufferSize = 65536;
                realTimeClient.Connect("192.168.1.75", 504);
                realTimeStream = realTimeClient.GetStream();
                realTimeStream.ReadTimeout = 100;

                byte[] buffer = new byte[65536];
                int bytesRead;

                while (isRealTimeMonitoring)
                {
                    try
                    {
                        if (realTimeStream.DataAvailable)
                        {
                            bytesRead = realTimeStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                lock (dataLock)
                                {
                                    for (int i = 0; i < bytesRead; i++)
                                    {
                                        bufferAccumulator.Add(buffer[i]);
                                    }
                                    
                                    ProcessCompleteFrames();
                                    
                                    if (receivedDataList.Count > 0)
                                    {
                                        latestFrameData = receivedDataList[receivedDataList.Count - 1];
                                    }
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (isRealTimeMonitoring)
                        {
                            UpdateRealTimeDisplayText("接收错误: " + ex.Message);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateRealTimeDisplayText("连接错误: " + ex.Message);
            }
            finally
            {
                if (realTimeStream != null)
                    realTimeStream.Close();
                if (realTimeClient != null)
                    realTimeClient.Close();
            }
        }

        private void UpdateRealTimeDisplay()
        {
            if (rtbRealTimeDisplay.InvokeRequired)
            {
                rtbRealTimeDisplay.Invoke(new Action(UpdateRealTimeDisplay));
                return;
            }

            byte[] currentFrame;
            lock (dataLock)
            {
                currentFrame = latestFrameData;
            }

            if (currentFrame.Length == 0)
            {
                return;
            }

            StringBuilder display = new StringBuilder();
            display.AppendLine("实时数据监控 - " + DateTime.Now.ToString("HH:mm:ss.fff"));
            display.AppendLine(new string('-', 80));

            if (currentFrame.Length >= 6)
            {
                ushort frameCount = (ushort)((currentFrame[5] << 8) | currentFrame[4]);
                display.AppendLine($"帧计数: {frameCount}");
            }

            display.AppendLine();
            display.AppendLine("12组模拟量:");
            
            for (int row = 0; row < 3; row++)
            {
                display.Append("  ");
                for (int col = 0; col < 4; col++)
                {
                    int i = row * 4 + col;
                    int pos = 6 + i * 2;
                    if (pos + 1 < currentFrame.Length)
                    {
                        ushort value = (ushort)((currentFrame[pos + 1] << 8) | currentFrame[pos]);
                        display.Append($"[{i+1:00}]:{value,5}(0x{value:X4})  ");
                    }
                }
                display.AppendLine();
            }

            display.AppendLine();
            display.AppendLine("4组编码器:");
            display.Append("  ");
            for (int i = 0; i < 4; i++)
            {
                int pos = 6 + 24 + i * 4;
                if (pos + 3 < currentFrame.Length)
                {
                    short high = (short)((currentFrame[pos + 1] << 8) | currentFrame[pos]);
                    ushort low = (ushort)((currentFrame[pos + 3] << 8) | currentFrame[pos + 2]);
                    int encoderValue = high * 60000 + low;
                    display.Append($"[{i+1}]:{encoderValue,12}(高:0x{high:X4},低:0x{low:X4})  ");
                }
            }
            display.AppendLine();

            rtbRealTimeDisplay.Clear();
            rtbRealTimeDisplay.AppendText(display.ToString());
        }

        private void UpdateRealTimeDisplayText(string text)
        {
            if (rtbRealTimeDisplay.InvokeRequired)
            {
                rtbRealTimeDisplay.Invoke(new Action<string>(UpdateRealTimeDisplayText), text);
            }
            else
            {
                rtbRealTimeDisplay.AppendText(text + Environment.NewLine);
                rtbRealTimeDisplay.ScrollToCaret();
            }
        }

        private void SetupDataGridViewColumns()
        {
            dataGridView1.Columns.Clear();
            
            // 添加基本信息列
            dataGridView1.Columns.Add("Frame", "帧");
            dataGridView1.Columns.Add("Group", "组");
            dataGridView1.Columns.Add("FrameCount", "帧计数");
            
            // 添加模拟量列
            for (int i = 0; i < 12; i++)
            {
                dataGridView1.Columns.Add($"Analog{i+1}", $"模拟量{i+1}");
                dataGridView1.Columns.Add($"Analog{i+1}Hex", "十六进制");
            }
            
            // 添加编码器列
            for (int i = 0; i < 4; i++)
            {
                dataGridView1.Columns.Add($"Encoder{i+1}", $"编码器{i+1}");
                dataGridView1.Columns.Add($"Encoder{i+1}High", "高位");
                dataGridView1.Columns.Add($"Encoder{i+1}Low", "低位");
            }
            
            // 设置列宽
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportDataToExcel();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            ExportDataToCsv();
        }

        private void btnExportTxt_Click(object sender, EventArgs e)
        {
            ExportDataToTxt();
        }

        private void ExportDataToExcel()
        {
            try
            {
                // 检查是否有解析数据
                if (dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("没有数据可导出，请先解析数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 设置EPPlus许可证
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // 创建SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel文件 (*.xlsx)|*.xlsx",
                    Title = "导出数据到Excel",
                    FileName = $"解析数据_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 创建Excel包
                    using (ExcelPackage excelPackage = new ExcelPackage())
                    {
                        // 创建工作表
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("解析数据");

                        // 添加表头
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = dataGridView1.Columns[i].HeaderText;
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        }

                        // 添加数据行
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            for (int j = 0; j < dataGridView1.Columns.Count; j++)
                            {
                                if (dataGridView1.Rows[i].Cells[j].Value != null)
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                                }
                            }
                        }

                        // 自动调整列宽
                        worksheet.Cells.AutoFitColumns();

                        // 保存文件
                        FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                        excelPackage.SaveAs(excelFile);

                        MessageBox.Show($"数据已成功导出到：{saveFileDialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出Excel失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDataToCsv()
        {
            try
            {
                // 检查是否有数据可导出
                if (parsedDataList == null || parsedDataList.Count == 0)
                {
                    MessageBox.Show("没有数据可导出，请先解析数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV文件 (*.csv)|*.csv",
                    Title = "导出数据到CSV",
                    FileName = $"解析数据_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 创建并写入CSV文件
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // 写入表头
                        writer.Write("\"帧序号\",\"组序号\",\"帧计数\"");
                        for (int i = 0; i < 12; i++)
                        {
                            writer.Write($",\"模拟量{i+1}\",\"模拟量{i+1}(Hex)\"");
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            writer.Write($",\"编码器{i+1}\",\"编码器{i+1}(高)\",\"编码器{i+1}(低)\"");
                        }
                        writer.WriteLine();

                        // 写入数据行
                        foreach (ParseDataRow row in parsedDataList)
                        {
                            writer.Write($"\"{row.FrameIndex}\",\"{row.GroupIndex}\",\"{row.FrameCount}\"");
                            
                            for (int i = 0; i < 12; i++)
                            {
                                writer.Write($",\"{row.AnalogValues[i]}\",\"0x{row.AnalogValues[i]:X4}\"");
                            }
                            
                            for (int i = 0; i < 4; i++)
                            {
                                writer.Write($",\"{row.EncoderValues[i]}\",\"0x{row.EncoderHigh[i]:X4}\",\"0x{row.EncoderLow[i]:X4}\"");
                            }
                            writer.WriteLine();
                        }
                    }

                    MessageBox.Show($"数据已成功导出到：{saveFileDialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出CSV失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDataToTxt()
        {
            try
            {
                // 检查是否有数据可导出
                if (receivedDataList == null || receivedDataList.Count == 0)
                {
                    MessageBox.Show("没有数据可导出，请先接收数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "TXT文件 (*.txt)|*.txt",
                    Title = "导出原始数据到TXT",
                    FileName = $"原始数据_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 创建并写入TXT文件
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine($"总计接收：{totalBytesReceived} 字节");
                        writer.WriteLine($"帧数量：{receivedDataList.Count}");
                        if (receivedDataList.Count > 0)
                        {
                            double avgBytesPerFrame = (double)totalBytesReceived / receivedDataList.Count;
                            writer.WriteLine($"平均每帧：{avgBytesPerFrame:F2} 字节");
                            writer.WriteLine($"预期比例：{FRAME_SIZE} 字节/帧");
                        }
                        if (bufferAccumulator.Count > 0)
                        {
                            writer.WriteLine($"剩余未处理数据：{bufferAccumulator.Count} 字节");
                        }
                        
                        writer.WriteLine();
                        writer.WriteLine("原始16进制数据：");
                        writer.WriteLine(new string('-', 80));
                        
                        int frameIndex = 0;
                        foreach (byte[] frameData in receivedDataList)
                        {
                            writer.WriteLine($"帧 {frameIndex + 1} ({frameData.Length} 字节)：");
                            string hexData = ByteArrayToHexString(frameData, frameData.Length);
                            writer.WriteLine(hexData);
                            writer.WriteLine(new string('-', 80));
                            frameIndex++;
                        }
                    }

                    MessageBox.Show($"数据已成功导出到：{saveFileDialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出TXT失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessCompleteFrames()
        {
            // Process frames based on 0xffffffff frame header
            while (bufferAccumulator.Count >= 4) // Need at least 4 bytes to check for frame header
            {
                // Look for 0xffffffff frame header
                int frameHeaderIndex = FindFrameHeader(bufferAccumulator);
                
                if (frameHeaderIndex >= 0)
                {
                    // Check if we have enough data for a complete frame
                    if (bufferAccumulator.Count >= frameHeaderIndex + FRAME_SIZE)
                    {
                        // Extract frame data
                        byte[] frameData = new byte[FRAME_SIZE];
                        for (int i = 0; i < FRAME_SIZE; i++)
                        {
                            frameData[i] = bufferAccumulator[frameHeaderIndex + i];
                        }
                        
                        // Remove processed data from accumulator
                        // Remove from the start of the buffer up to the end of this frame
                        bufferAccumulator.RemoveRange(0, frameHeaderIndex + FRAME_SIZE);
                        
                        // Add to received data list
                        receivedDataList.Add(frameData);
                        totalBytesReceived += FRAME_SIZE;
                    }
                    else
                    {
                        // Not enough data for a complete frame, but if there's data before the header,
                        // remove it to prevent processing junk data
                        if (frameHeaderIndex > 0)
                        {
                            bufferAccumulator.RemoveRange(0, frameHeaderIndex);
                        }
                        // Break and wait for more data
                        break;
                    }
                }
                else
                {
                    // No frame header found, remove all data up to the end
                    // This handles any junk data before the first frame header
                    bufferAccumulator.Clear();
                    break;
                }
            }
        }
        
        private int FindFrameHeader(List<byte> data)
        {
            // Look for 0xffffffff frame header (4 bytes)
            for (int i = 0; i <= data.Count - 4; i++)
            {
                if (data[i] == 0xFF && data[i + 1] == 0xFF && data[i + 2] == 0xFF && data[i + 3] == 0xFF)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public class ParseDataRow
    {
        public int FrameIndex { get; set; }
        public int GroupIndex { get; set; }
        public ushort FrameCount { get; set; }
        public ushort[] AnalogValues { get; set; }
        public int[] EncoderValues { get; set; }
        public short[] EncoderHigh { get; set; }
        public ushort[] EncoderLow { get; set; }
    }
}
