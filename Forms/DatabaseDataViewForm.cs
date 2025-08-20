using DataVisualizationApp.Database.Entities;
using DataVisualizationApp.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataVisualizationApp.Forms
{
    public partial class DatabaseDataViewForm : Form
    {
        // 数据服务
        private ExcelLibraryService _excelLibraryService;
        // 当前页码
        private int _currentPage = 1;
        // 每页记录数
        private int _pageSize = 50;
        // 总记录数
        private int _totalRecords = 0;
        // 总页数
        private int _totalPages => (_totalRecords + _pageSize - 1) / _pageSize;
        // 当前选择的Excel文件
        private ExcelFile? _selectedExcelFile;
        // 当前选择的工作表
        private ExcelSheet? _selectedSheet;

        public DatabaseDataViewForm(ExcelLibraryService excelLibraryService)
        {
            _excelLibraryService = excelLibraryService;
            InitializeComponent();
            this.Text = "数据库数据查看";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            // 加载Excel文件
            LoadExcelFiles();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 创建主容器TableLayoutPanel
            TableLayoutPanel mainContainer = new TableLayoutPanel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.ColumnCount = 1;
            mainContainer.RowCount = 4;
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 文件选择面板
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 工作表选择面板
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 主面板
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // 分页控件

            // 创建文件选择面板
            CreateFileSelectionPanel();
            mainContainer.Controls.Add(fileSelectionPanel, 0, 0);

            // 创建工作表选择面板
            CreateSheetSelectionPanel();
            mainContainer.Controls.Add(sheetSelectionPanel, 0, 1);

            // 创建主面板
            CreateMainPanel();
            mainContainer.Controls.Add(mainPanel, 0, 2);

            // 创建分页控件
            CreatePaginationControls();
            mainContainer.Controls.Add(paginationPanel, 0, 3);

            this.Controls.Add(mainContainer);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region 文件选择面板
        private Panel fileSelectionPanel = null!;
        private ComboBox fileComboBox = null!;
        private Label fileLabel = null!;
        private Button refreshFilesButton = null!;
        private Button deleteFileButton = null!;

        private void CreateFileSelectionPanel()
        {
            fileSelectionPanel = new Panel();
            fileSelectionPanel.Dock = DockStyle.Fill;
            fileSelectionPanel.BackColor = Color.FromArgb(224, 224, 224);
            fileSelectionPanel.Padding = new Padding(10);

            // 文件标签
            fileLabel = new Label();
            fileLabel.Text = "选择Excel文件: ";
            fileLabel.Location = new Point(10, 15);
            fileLabel.AutoSize = true;

            // 文件下拉框
            fileComboBox = new ComboBox();
            fileComboBox.Size = new Size(500, 23);  // 增加宽度以显示更长的文件名
            fileComboBox.Location = new Point(fileLabel.Right + 15, 12);  // 增加与标签的距离，确保显示完整
            fileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            fileComboBox.SelectedIndexChanged += FileComboBox_SelectedIndexChanged;

            // 刷新按钮
            refreshFilesButton = new Button();
            refreshFilesButton.Text = "刷新";
            refreshFilesButton.Size = new Size(70, 25);
            refreshFilesButton.Location = new Point(fileComboBox.Right + 10, 12);
            refreshFilesButton.Click += RefreshFilesButton_Click;

            // 删除文件按钮
            deleteFileButton = new Button();
            deleteFileButton.Text = "删除文件";            
            deleteFileButton.Size = new Size(80, 25);
            deleteFileButton.Location = new Point(refreshFilesButton.Right + 10, 12);
            deleteFileButton.Click += DeleteFileButton_Click;

            // 添加控件到面板
            fileSelectionPanel.Controls.Add(fileLabel);
            fileSelectionPanel.Controls.Add(fileComboBox);
            fileSelectionPanel.Controls.Add(refreshFilesButton);
            fileSelectionPanel.Controls.Add(deleteFileButton);

            this.Controls.Add(fileSelectionPanel);
        }

        private void LoadExcelFiles()
        {
            try
            {
                var files = _excelLibraryService.GetAllExcelFiles();
                fileComboBox.Items.Clear();

                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        fileComboBox.Items.Add(new FileComboBoxItem(file));
                    }
                    fileComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FileComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (fileComboBox.SelectedItem is FileComboBoxItem item)
            {
                _selectedExcelFile = item.File;
                _currentPage = 1;
                LoadSheets();
            }
        }

        private void RefreshFilesButton_Click(object? sender, EventArgs e)
        {
            LoadExcelFiles();
        }

        private void DeleteFileButton_Click(object? sender, EventArgs e)
        {
            if (_selectedExcelFile == null)
            {
                MessageBox.Show("请先选择要删除的Excel文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                DialogResult result = MessageBox.Show($"确定要删除文件 '{_selectedExcelFile.FileName}' 及其所有关联的工作表数据吗？\n\n此操作不可撤销！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    // 删除Excel文件及其关联数据
                    _excelLibraryService.DeleteExcelFile(_selectedExcelFile.Id);

                    // 刷新文件列表
                    LoadExcelFiles();

                    // 清空当前选择
                    _selectedExcelFile = null;
                    _selectedSheet = null;
                    sheetComboBox.Items.Clear();
                    dataGridView.DataSource = null;

                    MessageBox.Show("文件删除成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
             {
                MessageBox.Show($"删除文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
        }

        // 用于在ComboBox中显示文件信息的包装类
        private class FileComboBoxItem
        {
            public ExcelFile File { get; }

            public FileComboBoxItem(ExcelFile file)
            {
                File = file;
            }

            public override string ToString()
            {
                return $"{File.FileName} ({File.ImportDate:yyyy-MM-dd})";
            }
        }
        #endregion

        #region 工作表选择面板
        private Panel sheetSelectionPanel = null!;
        private ComboBox sheetComboBox = null!;
        private Label sheetLabel = null!;
        private CheckBox hasHeadersCheckBox = null!;
        private Label headerRowsLabel = null!;
        private NumericUpDown headerRowsNumericUpDown = null!;

        private void CreateSheetSelectionPanel()
        {
            sheetSelectionPanel = new Panel();
            sheetSelectionPanel.Dock = DockStyle.Fill;
            sheetSelectionPanel.BackColor = Color.FromArgb(224, 224, 224);
            sheetSelectionPanel.Padding = new Padding(10);

            // 工作表标签
            sheetLabel = new Label();
            sheetLabel.Text = "选择工作表: ";
            sheetLabel.Location = new Point(10, 15);
            sheetLabel.AutoSize = true;

            // 工作表下拉框
            sheetComboBox = new ComboBox();
            sheetComboBox.Size = new Size(300, 23);
            sheetComboBox.Location = new Point(sheetLabel.Right + 5, 12);
            sheetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sheetComboBox.SelectedIndexChanged += SheetComboBox_SelectedIndexChanged;

            // 包含表头复选框
            hasHeadersCheckBox = new CheckBox();
            hasHeadersCheckBox.Text = "包含表头";
            hasHeadersCheckBox.Checked = true;
            hasHeadersCheckBox.Location = new Point(sheetComboBox.Right + 20, 15);
            hasHeadersCheckBox.CheckedChanged += HasHeadersCheckBox_CheckedChanged;

            // 表头行数标签
            headerRowsLabel = new Label();
            headerRowsLabel.Text = "表头行数: ";
            headerRowsLabel.Location = new Point(hasHeadersCheckBox.Right + 10, 15);
            headerRowsLabel.AutoSize = true;

            // 表头行数数字选择器
            headerRowsNumericUpDown = new NumericUpDown();
            headerRowsNumericUpDown.Minimum = 1;
            headerRowsNumericUpDown.Maximum = 10;
            headerRowsNumericUpDown.Value = 1;
            headerRowsNumericUpDown.Size = new Size(60, 23);
            headerRowsNumericUpDown.Location = new Point(headerRowsLabel.Right + 5, 12);
            headerRowsNumericUpDown.ValueChanged += HeaderRowsNumericUpDown_ValueChanged;

            // 添加控件到面板
            sheetSelectionPanel.Controls.Add(sheetLabel);
            sheetSelectionPanel.Controls.Add(sheetComboBox);
            sheetSelectionPanel.Controls.Add(hasHeadersCheckBox);
            sheetSelectionPanel.Controls.Add(headerRowsLabel);
            sheetSelectionPanel.Controls.Add(headerRowsNumericUpDown);

            this.Controls.Add(sheetSelectionPanel);
        }

        private void LoadSheets()
        {
            try
            {
                if (_selectedExcelFile != null)
                {
                    // 获取文件的所有工作表
                    var file = _excelLibraryService.GetExcelFileById(_selectedExcelFile.Id);
                    if (file != null && file.Sheets != null)
                    {
                        sheetComboBox.Items.Clear();
                        foreach (var sheet in file.Sheets)
                        {
                            sheetComboBox.Items.Add(new SheetComboBoxItem(sheet));
                        }
                        if (sheetComboBox.Items.Count > 0)
                        {
                            sheetComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载工作表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SheetComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sheetComboBox.SelectedItem is SheetComboBoxItem item)
            {
                _selectedSheet = item.Sheet;
                _currentPage = 1;
                LoadData();
            }
        }

        // 用于在ComboBox中显示工作表信息的包装类
        private class SheetComboBoxItem
        {
            public ExcelSheet Sheet { get; }

            public SheetComboBoxItem(ExcelSheet sheet)
            {
                Sheet = sheet;
            }

            public override string ToString()
            {
                return $"{Sheet.SheetName} (行: {Sheet.RowCount}, 列: {Sheet.ColumnCount})";
            }
        }
        #endregion

        #region 主面板
        private Panel mainPanel = null!;
        private DataGridView dataGridView = null!;
        private Label pageInfoLabel = null!;

        private void CreateMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);
            mainPanel.BackColor = Color.FromArgb(240, 240, 240);
            mainPanel.AutoScroll = true;

            // 创建DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dataGridView.RowHeadersVisible = false;

            mainPanel.Controls.Add(dataGridView);
            this.Controls.Add(mainPanel);
        }
        #endregion

        #region 数据加载与分页
        private void LoadData()
        {
            if (_selectedSheet == null || _selectedExcelFile == null)
                return;

            try
            {
                // 读取工作表数据
                DataTable data = _excelLibraryService.ReadExcelFileData(_selectedExcelFile.Id, _selectedSheet.SheetName);

                // 处理表头
                if (hasHeadersCheckBox.Checked)
                {
                    int headerRows = (int)headerRowsNumericUpDown.Value;
                    ProcessDataTableHeaders(data, headerRows);
                }

                // 更新总记录数
                _totalRecords = data.Rows.Count;

                // 更新分页信息
                UpdatePaginationInfo();

                // 计算分页数据
                int startIndex = (_currentPage - 1) * _pageSize;
                int endIndex = Math.Min(startIndex + _pageSize - 1, _totalRecords - 1);

                // 创建分页后的数据表
                DataTable pageData = data.Clone();
                for (int i = startIndex; i <= endIndex; i++)
                {
                    pageData.ImportRow(data.Rows[i]);
                }

                // 绑定数据
                dataGridView.DataSource = pageData;

                // 更新按钮状态
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HasHeadersCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            headerRowsLabel.Enabled = hasHeadersCheckBox.Checked;
            headerRowsNumericUpDown.Enabled = hasHeadersCheckBox.Checked;
            if (_selectedSheet != null)
            {
                LoadData();
            }
        }

        private void HeaderRowsNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_selectedSheet != null)
            {
                LoadData();
            }
        }
        #endregion

        #region 分页控件
        private Panel paginationPanel = null!;
        private Button firstPageButton = null!;
        private Button prevPageButton = null!;
        private Button nextPageButton = null!;
        private Button lastPageButton = null!;
        private NumericUpDown pageNumberInput = null!;
        private Label pageSeparatorLabel = null!;
        private Label totalPagesLabel = null!;
        private ComboBox pageSizeComboBox = null!;
        private Label pageSizeLabel = null!;

        private void CreatePaginationControls()
        {
            paginationPanel = new Panel();
            paginationPanel.Dock = DockStyle.Fill;
            paginationPanel.BackColor = Color.FromArgb(224, 224, 224);
            paginationPanel.Padding = new Padding(10);

            // 第一页按钮
            firstPageButton = new Button();
            firstPageButton.Text = "首页";
            firstPageButton.Size = new Size(70, 25);
            firstPageButton.Location = new Point(10, 7);
            firstPageButton.Click += FirstPageButton_Click;

            // 上一页按钮
            prevPageButton = new Button();
            prevPageButton.Text = "上一页";
            prevPageButton.Size = new Size(70, 25);
            prevPageButton.Location = new Point(firstPageButton.Right + 5, 7);
            prevPageButton.Click += PrevPageButton_Click;

            // 页码输入
            pageNumberInput = new NumericUpDown();
            pageNumberInput.Size = new Size(60, 25);
            pageNumberInput.Location = new Point(prevPageButton.Right + 5, 7);
            pageNumberInput.Minimum = 1;
            pageNumberInput.Maximum = 1;
            pageNumberInput.Value = 1;
            pageNumberInput.ValueChanged += PageNumberInput_ValueChanged;

            // 分隔符
            pageSeparatorLabel = new Label();
            pageSeparatorLabel.Text = "/";
            pageSeparatorLabel.Location = new Point(pageNumberInput.Right + 2, 10);

            // 总页数
            totalPagesLabel = new Label();
            totalPagesLabel.Text = "1";
            totalPagesLabel.Location = new Point(pageSeparatorLabel.Right + 2, 10);

            // 下一页按钮
            nextPageButton = new Button();
            nextPageButton.Text = "下一页";
            nextPageButton.Size = new Size(70, 25);
            nextPageButton.Location = new Point(totalPagesLabel.Right + 5, 7);
            nextPageButton.Click += NextPageButton_Click;

            // 最后一页按钮
            lastPageButton = new Button();
            lastPageButton.Text = "末页";
            lastPageButton.Size = new Size(70, 25);
            lastPageButton.Location = new Point(nextPageButton.Right + 5, 7);
            lastPageButton.Click += LastPageButton_Click;

            // 页大小标签
            pageSizeLabel = new Label();
            pageSizeLabel.Text = "每页记录数:";
            pageSizeLabel.Location = new Point(lastPageButton.Right + 30, 11);

            // 页大小下拉框
            pageSizeComboBox = new ComboBox();
            pageSizeComboBox.Size = new Size(60, 23);
            pageSizeComboBox.Location = new Point(pageSizeLabel.Right + 5, 8);
            pageSizeComboBox.Items.AddRange(new object[] { 20, 50, 100, 200 });
            pageSizeComboBox.SelectedItem = 50;
            pageSizeComboBox.SelectedIndexChanged += PageSizeComboBox_SelectedIndexChanged;

            // 页面信息
            pageInfoLabel = new Label();
            pageInfoLabel.Text = "共 0 条记录";
            pageInfoLabel.AutoSize = true;
            pageInfoLabel.Location = new Point(pageSizeComboBox.Right + 30, 11);

            // 添加控件到面板
            paginationPanel.Controls.Add(firstPageButton);
            paginationPanel.Controls.Add(prevPageButton);
            paginationPanel.Controls.Add(pageNumberInput);
            paginationPanel.Controls.Add(pageSeparatorLabel);
            paginationPanel.Controls.Add(totalPagesLabel);
            paginationPanel.Controls.Add(nextPageButton);
            paginationPanel.Controls.Add(lastPageButton);
            paginationPanel.Controls.Add(pageSizeLabel);
            paginationPanel.Controls.Add(pageSizeComboBox);
            paginationPanel.Controls.Add(pageInfoLabel);
            // 不再直接添加到Form，而是在InitializeComponent中添加到TableLayoutPanel
            // this.Controls.Add(paginationPanel);
        }

        private void UpdatePaginationInfo()
        {
            totalPagesLabel.Text = _totalPages.ToString();
            pageNumberInput.Maximum = _totalPages;
            pageInfoLabel.Text = $"共 {_totalRecords} 条记录";
        }

        private void UpdateButtonStates()
        {
            firstPageButton.Enabled = _currentPage > 1;
            prevPageButton.Enabled = _currentPage > 1;
            nextPageButton.Enabled = _currentPage < _totalPages;
            lastPageButton.Enabled = _currentPage < _totalPages;
        }

        private void FirstPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                LoadData();
            }
        }

        private void PrevPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadData();
            }
        }

        private void NextPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadData();
            }
        }

        private void LastPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage != _totalPages)
            {
                _currentPage = _totalPages;
                LoadData();
            }
        }

        private void PageNumberInput_ValueChanged(object? sender, EventArgs e)
        {
            _currentPage = (int)pageNumberInput.Value;
            LoadData();
        }

        private void PageSizeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (pageSizeComboBox.SelectedItem != null)
            {
                _pageSize = Convert.ToInt32(pageSizeComboBox.SelectedItem);
                _currentPage = 1;
                LoadData();
            }
        }
        #endregion

        #region 表头处理
        /// <summary>
        /// 处理DataTable的多行表头，合并为单行表头
        /// </summary>
        /// <param name="dataTable">要处理的DataTable</param>
        /// <param name="headerRowCount">表头行数</param>
        private void ProcessDataTableHeaders(DataTable dataTable, int headerRowCount)
        {
            // 调整表头行数：用户选择的行数减1，以匹配用户预期
            // 例如：用户选择1行表头，实际处理1行而不是2行
            int adjustedHeaderRowCount = Math.Max(0, headerRowCount - 1);

            if (dataTable == null || dataTable.Rows.Count < adjustedHeaderRowCount)
                return;

            // 存储新的列名
            var newColumnNames = new List<string>();
            int columnCount = dataTable.Columns.Count;

            // 处理每一列的表头
            for (int col = 0; col < columnCount; col++)
            {
                var headerParts = new List<string>();

                // 收集该列的所有表头行值
                for (int row = 0; row < adjustedHeaderRowCount; row++)
                {
                    var cellValue = dataTable.Rows[row][col];
                    if (cellValue != DBNull.Value)
                    {
                        string? valueStr = cellValue.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(valueStr))
                        {
                            headerParts.Add(valueStr);
                        }
                    }
                }

                // 生成列名
                string headerName;
                if (headerParts.Any())
                {
                    // 合并多行表头，使用非重复的部分
                    headerName = string.Join("_", headerParts.Distinct());
                }
                else
                {
                    headerName = dataTable.Columns[col].ColumnName;
                }

                // 确保列名唯一
                string uniqueHeaderName = headerName;
                int counter = 1;
                while (newColumnNames.Contains(uniqueHeaderName))
                {
                    uniqueHeaderName = $"{headerName}_{counter}";
                    counter++;
                }
                newColumnNames.Add(uniqueHeaderName);
            }

            // 更新列名
            for (int col = 0; col < columnCount; col++)
            {
                dataTable.Columns[col].ColumnName = newColumnNames[col];
            }

            // 删除表头行
            if (adjustedHeaderRowCount > 0 && dataTable.Rows.Count >= adjustedHeaderRowCount)
            {
                DataRow[] rowsToDelete = dataTable.Rows.Cast<DataRow>().Take(adjustedHeaderRowCount).ToArray();
                foreach (DataRow row in rowsToDelete)
                {
                    row.Delete();
                }
            }
            dataTable.AcceptChanges();
        }
        #endregion
    }
}