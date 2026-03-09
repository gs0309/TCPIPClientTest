using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        private List<ParseDataRow> parseData;

        public Form2(List<ParseDataRow> data)
        {
            InitializeComponent();
            EnableDoubleBuffered(dataGridView1);
            parseData = data;
            LoadFormData();
        }

        private void EnableDoubleBuffered(DataGridView dgv)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (pi != null)
            {
                pi.SetValue(dgv, true, null);
            }
        }

        private void LoadFormData()
        {
            if (parseData == null || parseData.Count == 0)
            {
                MessageBox.Show("没有数据可显示", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetupDataGridViewColumns();

            dataGridView1.SuspendLayout();
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            try
            {
                dataGridView1.RowCount = parseData.Count;
                
                for (int i = 0; i < parseData.Count; i++)
                {
                    ParseDataRow row = parseData[i];
                    DataGridViewRow dgvRow = dataGridView1.Rows[i];
                    dgvRow.Cells[0].Value = row.FrameIndex;
                    dgvRow.Cells[1].Value = row.GroupIndex;
                    dgvRow.Cells[2].Value = row.FrameCount;
                    
                    int colIndex = 3;
                    for (int j = 0; j < 12; j++)
                    {
                        dgvRow.Cells[colIndex++].Value = row.AnalogValues[j];
                        dgvRow.Cells[colIndex++].Value = $"0x{row.AnalogValues[j]:X4}";
                    }
                    
                    for (int j = 0; j < 4; j++)
                    {
                        dgvRow.Cells[colIndex++].Value = row.EncoderValues[j];
                        dgvRow.Cells[colIndex++].Value = $"0x{row.EncoderHigh[j]:X4}";
                        dgvRow.Cells[colIndex++].Value = $"0x{row.EncoderLow[j]:X4}";
                    }
                }
            }
            finally
            {
                dataGridView1.ResumeLayout(true);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }

            Text = $"解析数据 - 共{parseData.Count}条记录";
        }

        private void SetupDataGridViewColumns()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.RowHeadersVisible = false;

            dataGridView1.Columns.Add("FrameIndex", "帧序号");
            dataGridView1.Columns.Add("GroupIndex", "组序号");
            dataGridView1.Columns.Add("FrameCount", "帧计数");

            for (int i = 0; i < 12; i++)
            {
                dataGridView1.Columns.Add($"Analog{i}", $"模拟量{i+1}");
                dataGridView1.Columns.Add($"Analog{i}Hex", $"模拟量{i+1}(Hex)");
            }

            for (int i = 0; i < 4; i++)
            {
                dataGridView1.Columns.Add($"Encoder{i}", $"编码器{i+1}");
                dataGridView1.Columns.Add($"Encoder{i}High", $"编码器{i+1}(高)");
                dataGridView1.Columns.Add($"Encoder{i}Low", $"编码器{i+1}(低)");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
