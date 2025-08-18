using DataVisualizationApp.Services;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataVisualizationApp.Forms
{
    public partial class DataImportForm : Form
    {
        private ExcelService excelService;
        private DataTable previewData;
        private string selectedFilePath;

        public DataImportForm()
        {
            InitializeComponent();
            excelService = new ExcelService();
            this.Text = "Excel数据导入";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            CreateMainLayout();
            CreateFileSelectionPanel();
            CreatePreviewPanel();
            CreateSettingsPanel();
            CreateButtonPanel();
            ResumeLayout(false);
            PerformLayout();
        }

        #region 界面控件
        private TableLayoutPanel mainLayout;

        // 文件选择区域
        private GroupBox fileGroupBox;
        private TextBox filePathTextBox;
        private Button browseButton;
        private Label fileInfoLabel;

        // 工作表选择
        private Label worksheetLabel;
        private ComboBox worksheetComboBox;

        // 预览区域
        private GroupBox previewGroupBox;
        private DataGridView previewDataGridView;
        private Label previewInfoLabel;

        // 设置区域
        private GroupBox settingsGroupBox;
        private CheckBox hasHeadersCheckBox;
        private Label headerRowsLabel;
        private NumericUpDown headerRowsNumericUpDown;
        private Button detectTypesButton;
        private ListView columnTypesListView;

        // 按钮区域
        private Panel buttonPanel;
        private Button importButton;
        private Button cancelButton;
        private ProgressBar progressBar;
        #endregion

        #region 创建界面
        private void CreateMainLayout()
        {
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 4;
            mainLayout.ColumnCount = 2;

            // 设置行高
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 文件选择
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // 预览
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // 设置
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 按钮

            // 设置列宽
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            this.Controls.Add(mainLayout);
        }

        private void CreateFileSelectionPanel()
        {
            fileGroupBox = new GroupBox();
            fileGroupBox.Text = "选择Excel文件";
            fileGroupBox.Dock = DockStyle.Fill;
            fileGroupBox.Margin = new Padding(5);
            mainLayout.Controls.Add(fileGroupBox, 0, 0);
            mainLayout.SetColumnSpan(fileGroupBox, 2);

            var filePanel = new TableLayoutPanel();
            filePanel.Dock = DockStyle.Fill;
            filePanel.RowCount = 3;
            filePanel.ColumnCount = 3;
            filePanel.Padding = new Padding(10);

            filePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            filePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            filePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // 文件路径输入
            filePathTextBox = new TextBox();
            filePathTextBox.Dock = DockStyle.Fill;
            filePathTextBox.ReadOnly = true;
            filePathTextBox.BackColor = Color.White;
            filePanel.Controls.Add(filePathTextBox, 0, 0);

            browseButton = new Button();
            browseButton.Text = "浏览...";
            browseButton.Size = new Size(80, 23);
            browseButton.Anchor = AnchorStyles.Left;
            browseButton.Click += BrowseButton_Click;
            filePanel.Controls.Add(browseButton, 1, 0);

            // 文件信息
            fileInfoLabel = new Label();
            fileInfoLabel.Text = "请选择Excel文件";
            fileInfoLabel.Dock = DockStyle.Fill;
            fileInfoLabel.ForeColor = Color.Gray;
            filePanel.Controls.Add(fileInfoLabel, 0, 1);
            filePanel.SetColumnSpan(fileInfoLabel, 3);

            // 工作表选择
            worksheetLabel = new Label();
            worksheetLabel.Text = "工作表:";
            worksheetLabel.Anchor = AnchorStyles.Left;
            worksheetLabel.Visible = false;
            filePanel.Controls.Add(worksheetLabel, 0, 2);

            worksheetComboBox = new ComboBox();
            worksheetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            worksheetComboBox.Dock = DockStyle.Fill;
            worksheetComboBox.Visible = false;
            worksheetComboBox.SelectedIndexChanged += WorksheetComboBox_SelectedIndexChanged;
            filePanel.Controls.Add(worksheetComboBox, 1, 2);
            filePanel.SetColumnSpan(worksheetComboBox, 2);

            fileGroupBox.Controls.Add(filePanel);
        }

        private void CreatePreviewPanel()
        {
            previewGroupBox = new GroupBox();
            previewGroupBox.Text = "数据预览";
            previewGroupBox.Dock = DockStyle.Fill;
            previewGroupBox.Margin = new Padding(5);
            mainLayout.Controls.Add(previewGroupBox, 0, 1);

            var previewLayout = new TableLayoutPanel();
            previewLayout.Dock = DockStyle.Fill;
            previewLayout.RowCount = 2;
            previewLayout.ColumnCount = 1;
            previewLayout.Padding = new Padding(5);
            previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            previewLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            previewDataGridView = new DataGridView();
            previewDataGridView.Dock = DockStyle.Fill;
            previewDataGridView.ReadOnly = true;
            previewDataGridView.AllowUserToAddRows = false;
            previewDataGridView.AllowUserToDeleteRows = false;
            previewDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            previewDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            previewDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            previewLayout.Controls.Add(previewDataGridView, 0, 0);

            previewInfoLabel = new Label();
            previewInfoLabel.Text = "请选择Excel文件进行预览";
            previewInfoLabel.Dock = DockStyle.Fill;
            previewInfoLabel.ForeColor = Color.Gray;
            previewInfoLabel.Padding = new Padding(5);
            previewLayout.Controls.Add(previewInfoLabel, 0, 1);

            previewGroupBox.Controls.Add(previewLayout);
        }

        private void CreateSettingsPanel()
        {
            settingsGroupBox = new GroupBox();
            settingsGroupBox.Text = "导入设置";
            settingsGroupBox.Dock = DockStyle.Fill;
            settingsGroupBox.Margin = new Padding(5);
            mainLayout.Controls.Add(settingsGroupBox, 1, 1);
            mainLayout.SetRowSpan(settingsGroupBox, 2);

            var settingsLayout = new TableLayoutPanel();
            settingsLayout.Dock = DockStyle.Fill;
            settingsLayout.RowCount = 5;
            settingsLayout.ColumnCount = 2;
            settingsLayout.Padding = new Padding(5);

            for (int i = 0; i < 5; i++)
            {
                settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // 表头设置
            hasHeadersCheckBox = new CheckBox();
            hasHeadersCheckBox.Text = "包含表头";
            hasHeadersCheckBox.Checked = true;
            hasHeadersCheckBox.CheckedChanged += HasHeadersCheckBox_CheckedChanged;
            settingsLayout.Controls.Add(hasHeadersCheckBox, 0, 0);
            settingsLayout.SetColumnSpan(hasHeadersCheckBox, 2);

            headerRowsLabel = new Label();
            headerRowsLabel.Text = "表头行数:";
            headerRowsLabel.Anchor = AnchorStyles.Left;
            settingsLayout.Controls.Add(headerRowsLabel, 0, 1);

            headerRowsNumericUpDown = new NumericUpDown();
            headerRowsNumericUpDown.Minimum = 1;
            headerRowsNumericUpDown.Maximum = 10;
            headerRowsNumericUpDown.Value = 1;
            headerRowsNumericUpDown.ValueChanged += HeaderRowsNumericUpDown_ValueChanged;
            settingsLayout.Controls.Add(headerRowsNumericUpDown, 1, 1);

            // 数据类型检测
            detectTypesButton = new Button();
            detectTypesButton.Text = "检测列类型";
            detectTypesButton.Dock = DockStyle.Fill;
            detectTypesButton.Click += DetectTypesButton_Click;
            settingsLayout.Controls.Add(detectTypesButton, 0, 2);
            settingsLayout.SetColumnSpan(detectTypesButton, 2);

            // 列类型列表
            columnTypesListView = new ListView();
            columnTypesListView.View = View.Details;
            columnTypesListView.FullRowSelect = true;
            columnTypesListView.GridLines = true;
            columnTypesListView.Columns.Add("列名", 120);
            columnTypesListView.Columns.Add("数据类型", 80);
            columnTypesListView.Dock = DockStyle.Fill;
            settingsLayout.Controls.Add(columnTypesListView, 0, 3);
            settingsLayout.SetColumnSpan(columnTypesListView, 2);
            settingsLayout.SetRowSpan(columnTypesListView, 2);

            settingsGroupBox.Controls.Add(settingsLayout);
        }

        private void CreateButtonPanel()
        {
            buttonPanel = new Panel();
            buttonPanel.Height = 60;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Margin = new Padding(5);
            mainLayout.Controls.Add(buttonPanel, 0, 3);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            importButton = new Button();
            importButton.Text = "导入数据";
            importButton.Size = new Size(100, 30);
            importButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            importButton.Location = new Point(buttonPanel.Width - 220, 15);
            importButton.BackColor = Color.FromArgb(0, 120, 215);
            importButton.ForeColor = Color.White;
            importButton.FlatStyle = FlatStyle.Flat;
            importButton.Enabled = false;
            importButton.Click += ImportButton_Click;
            buttonPanel.Controls.Add(importButton);

            cancelButton = new Button();
            cancelButton.Text = "取消";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Location = new Point(buttonPanel.Width - 110, 15);
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Controls.Add(cancelButton);

            progressBar = new ProgressBar();
            progressBar.Size = new Size(300, 20);
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            progressBar.Location = new Point(10, 20);
            progressBar.Visible = false;
            buttonPanel.Controls.Add(progressBar);

            buttonPanel.Resize += (s, e) => {
                importButton.Location = new Point(buttonPanel.Width - 220, 15);
                cancelButton.Location = new Point(buttonPanel.Width - 110, 15);
            };
        }
        #endregion

        #region 事件处理
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "选择Excel文件";
                openFileDialog.Filter = "Excel文件|*.xlsx;*.xls|Excel 2007-2019|*.xlsx|Excel 97-2003|*.xls";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    LoadExcelFile();
                }
            }
        }

        private void LoadExcelFile()
        {
            try
            {
                filePathTextBox.Text = selectedFilePath;

                // 获取文件信息
                var excelInfo = excelService.GetExcelInfo(selectedFilePath);
                fileInfoLabel.Text = $"文件大小: {FormatFileSize(excelInfo.FileSize)}, " +
                $"行数: {excelInfo.RowCount}, 列数: {excelInfo.ColumnCount}, " +
                $"修改时间: {excelInfo.LastModified:yyyy-MM-dd HH:mm}";

                // 加载工作表列表
                worksheetComboBox.Items.Clear();
                worksheetComboBox.Items.AddRange(excelInfo.WorksheetNames.ToArray());
                worksheetComboBox.SelectedIndex = 0;

                // 显示工作表选择
                if (excelInfo.WorksheetNames.Count > 1)
                {
                    worksheetLabel.Visible = true;
                    worksheetComboBox.Visible = true;
                }

                LoadPreviewData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载Excel文件失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WorksheetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (worksheetComboBox.SelectedIndex >= 0)
            {
                LoadPreviewData();
            }
        }

        private void LoadPreviewData()
        {
            try
            {
                if (string.IsNullOrEmpty(selectedFilePath)) return;

                string worksheetName = worksheetComboBox.SelectedItem?.ToString();
                previewData = excelService.PreviewExcelData(selectedFilePath, worksheetName, 20);

                previewDataGridView.DataSource = previewData;
                previewInfoLabel.Text = $"预览数据 (显示前20行，共{previewData.Columns.Count}列)";

                importButton.Enabled = previewData.Rows.Count > 0;

                // 自动检测列类型
                DetectColumnTypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"预览数据失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HasHeadersCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            headerRowsLabel.Enabled = hasHeadersCheckBox.Checked;
            headerRowsNumericUpDown.Enabled = hasHeadersCheckBox.Checked;
            LoadPreviewData();
        }

        private void HeaderRowsNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            LoadPreviewData();
        }

        private void DetectTypesButton_Click(object sender, EventArgs e)
        {
            DetectColumnTypes();
        }

        private void DetectColumnTypes()
        {
            if (previewData == null) return;

            try
            {
                var columnTypes = excelService.DetectColumnTypes(previewData);

                columnTypesListView.Items.Clear();
                foreach (var kvp in columnTypes)
                {
                    var item = new ListViewItem(kvp.Key);
                    item.SubItems.Add(GetTypeName(kvp.Value));
                    columnTypesListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检测列类型失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ImportButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || previewData == null)
            {
                MessageBox.Show("请先选择并预览Excel文件", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 显示进度条
                progressBar.Visible = true;
                progressBar.Value = 0;
                importButton.Enabled = false;
                cancelButton.Text = "取消";

                // 异步导入数据
                var progress = new Progress<int>(value => progressBar.Value = value);

                string worksheetName = worksheetComboBox.SelectedItem?.ToString();
                bool hasHeaders = hasHeadersCheckBox.Checked;
                int headerRows = (int)headerRowsNumericUpDown.Value;

                // 在后台线程中读取完整数据
                var fullData = await System.Threading.Tasks.Task.Run(() =>
                {
                    ((IProgress<int>)progress).Report(20);
                    return excelService.ReadExcelData(selectedFilePath, worksheetName, hasHeaders, headerRows);
                });

                ((IProgress<int>)progress).Report(60);

                // 这里将添加数据库存储逻辑
                await System.Threading.Tasks.Task.Run(() =>
                {
                    // TODO: 保存到数据库
                    System.Threading.Thread.Sleep(1000); // 模拟保存过程
                    ((IProgress<int>)progress).Report(100);
                });

                MessageBox.Show($"数据导入成功！\n共导入 {fullData.Rows.Count} 行数据，{fullData.Columns.Count} 列",
                "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 设置导入成功的结果
                this.ImportedData = fullData;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入数据失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                importButton.Enabled = true;
                cancelButton.Text = "取消";
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion

        #region 辅助方法
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetTypeName(Type type)
        {
            if (type == typeof(int)) return "整数";
            if (type == typeof(double)) return "小数";
            if (type == typeof(DateTime)) return "日期";
            if (type == typeof(bool)) return "布尔";
            return "文本";
        }
        #endregion

        #region 公共属性
        ///

        /// 导入的数据
        ///

        public DataTable ImportedData { get; private set; }
        #endregion
    }
}
