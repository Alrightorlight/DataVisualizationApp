using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataVisualizationApp.Forms
{
    public partial class DataViewForm : Form
    {
        // 数据源
        private DataTable _dataSource = null!;
        // 当前页码
        private int _currentPage = 1;
        // 每页记录数
        private int _pageSize = 50;
        // 总页数
        private int _totalPages => ((_dataSource?.Rows.Count ?? 0) + _pageSize - 1) / _pageSize;

        public DataViewForm(DataTable dataSource)
        {
            if (dataSource == null || dataSource.Rows.Count == 0)
                throw new ArgumentNullException(nameof(dataSource), "数据源不能为空");

            _dataSource = dataSource;
            _originalDataSource = dataSource.Copy();  // 保存原始数据源
            InitializeComponent();
            this.Text = "数据查看";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 创建主面板
            CreateMainPanel();

            // 创建分页控件
            CreatePaginationControls();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region 主面板
        private Panel mainPanel = null!;
        private DataGridView dataGridView = null!;
        private Label pageInfoLabel = null!;
        private Panel filterPanel = null!;
        private ComboBox filterColumnComboBox = null!;
        private ComboBox filterValueComboBox = null!;
        private Button applyFilterButton = null!;
        private Button clearFilterButton = null!;
        private Label filterLabel = null!;
        private DataTable _originalDataSource = null!;  // 原始数据源，用于清除筛选
        private string? _currentFilterColumn = null;
        private string? _currentFilterValue = null;

        private void CreateMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);
            mainPanel.BackColor = Color.FromArgb(240, 240, 240);
            mainPanel.AutoScroll = true;

            // 使用TableLayoutPanel来精确控制布局
            TableLayoutPanel layoutPanel = new TableLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.ColumnCount = 1;
            layoutPanel.RowCount = 2;
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 筛选面板行
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // DataGridView行

            // 创建筛选面板
            filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.BackColor = Color.FromArgb(224, 224, 224);
            filterPanel.Padding = new Padding(10);

            // 筛选标签
            filterLabel = new Label();
            filterLabel.Text = "筛选条件: ";
            filterLabel.Location = new Point(10, 15);
            filterLabel.AutoSize = true;

            // 列名下拉框
            filterColumnComboBox = new ComboBox();
            filterColumnComboBox.Size = new Size(200, 23);  // 增加宽度以显示长标签
            filterColumnComboBox.DropDownWidth = 400;  // 设置下拉列表宽度
            filterColumnComboBox.Location = new Point(filterLabel.Right + 5, 12);
            filterColumnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            filterColumnComboBox.SelectedIndexChanged += FilterColumnComboBox_SelectedIndexChanged;

            // 值下拉框
            filterValueComboBox = new ComboBox();
            filterValueComboBox.Size = new Size(120, 23);
            filterValueComboBox.Location = new Point(filterColumnComboBox.Right + 10, 12);
            filterValueComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            // 应用筛选按钮
            applyFilterButton = new Button();
            applyFilterButton.Text = "应用筛选";
            applyFilterButton.Size = new Size(90, 25);
            applyFilterButton.Location = new Point(filterValueComboBox.Right + 10, 12);
            applyFilterButton.Click += ApplyFilterButton_Click;

            // 清除筛选按钮
            clearFilterButton = new Button();
            clearFilterButton.Text = "清除筛选";
            clearFilterButton.Size = new Size(90, 25);
            clearFilterButton.Location = new Point(applyFilterButton.Right + 10, 12);
            clearFilterButton.Click += ClearFilterButton_Click;
            clearFilterButton.Enabled = false;

            // 添加筛选控件到筛选面板
            filterPanel.Controls.Add(filterLabel);
            filterPanel.Controls.Add(filterColumnComboBox);
            filterPanel.Controls.Add(filterValueComboBox);
            filterPanel.Controls.Add(applyFilterButton);
            filterPanel.Controls.Add(clearFilterButton);

            // 创建DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.RowHeadersVisible = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AllowUserToOrderColumns = true;
            dataGridView.ColumnHeaderMouseClick += DataGridView_ColumnHeaderMouseClick;
            dataGridView.Sorted += DataGridView_Sorted;

            // 将控件添加到TableLayoutPanel
            layoutPanel.Controls.Add(filterPanel, 0, 0);
            layoutPanel.Controls.Add(dataGridView, 0, 1);

            // 添加TableLayoutPanel到主面板
            mainPanel.Controls.Add(layoutPanel);

            this.Controls.Add(mainPanel);
        }
        #endregion

        #region 排序功能
        private string? _sortColumn = null;
        private SortOrder _sortOrder = SortOrder.None;

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void DataGridView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                DataGridViewColumn column = dataGridView.Columns[e.ColumnIndex];

                // 如果点击的是当前排序列，则切换排序方向
                if (column.Name == _sortColumn)
                {
                    _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    // 否则，设置为新的排序列，默认升序
                    _sortColumn = column.Name;
                    _sortOrder = SortOrder.Ascending;
                }

                // 对整个数据源进行排序
                SortDataSource();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void DataGridView_Sorted(object? sender, EventArgs e)
        {
            // 清除所有列的排序图标
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            // 设置当前排序列的排序图标
            if (!string.IsNullOrEmpty(_sortColumn) && dataGridView.Columns.Contains(_sortColumn))
            {
                // 使用!运算符表示_sortColumn此时不为null
                dataGridView.Columns[_sortColumn!].HeaderCell.SortGlyphDirection = _sortOrder;
            }
        }

        private void SortDataSource()
        {
            if (_dataSource == null || _dataSource.Rows.Count == 0 || string.IsNullOrEmpty(_sortColumn))
                return;

            // 对整个数据源进行排序
            // 使用!运算符表示_sortColumn此时不为null
            _dataSource.DefaultView.Sort = $"{_sortColumn!} {( _sortOrder == SortOrder.Ascending ? "ASC" : "DESC" )}";
            _dataSource = _dataSource.DefaultView.ToTable();

            // 重新加载当前页数据
            LoadData();
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
            paginationPanel.Dock = DockStyle.Bottom;
            paginationPanel.Height = 40;
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
            pageInfoLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;

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

            this.Controls.Add(paginationPanel);
        }
        #endregion

        #region 数据加载与分页
        private void LoadData()
        {
            if (_dataSource == null || _dataSource.Rows.Count == 0)
                return;

            // 更新分页信息
            UpdatePaginationInfo();

            // 计算当前页的数据范围
            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize - 1, _dataSource.Rows.Count - 1);

            // 创建当前页的数据表
            DataTable pageData = _dataSource.Clone();
            for (int i = startIndex; i <= endIndex; i++)
            {
                pageData.ImportRow(_dataSource.Rows[i]);
            }

            // 绑定数据到DataGridView
            dataGridView.DataSource = pageData;

            // 更新按钮状态
            UpdateButtonStates();
        }

        private void UpdatePaginationInfo()
        {
            totalPagesLabel.Text = _totalPages.ToString();
            pageNumberInput.Maximum = _totalPages;
            pageInfoLabel.Text = $"共 {_dataSource.Rows.Count} 条记录";
        }

        private void UpdateButtonStates()
        {
            firstPageButton.Enabled = _currentPage > 1;
            prevPageButton.Enabled = _currentPage > 1;
            nextPageButton.Enabled = _currentPage < _totalPages;
            lastPageButton.Enabled = _currentPage < _totalPages;
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void FirstPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void PrevPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void NextPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void LastPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage != _totalPages)
            {
                _currentPage = _totalPages;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void PageNumberInput_ValueChanged(object? sender, EventArgs e)
        {
            int newPage = (int)pageNumberInput.Value;
            if (newPage != _currentPage && newPage >= 1 && newPage <= _totalPages)
            {
                _currentPage = newPage;
                LoadData();
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void PageSizeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (pageSizeComboBox.SelectedItem != null)
            {
                // 添加null检查以避免CS8622警告
                _pageSize = (int)(pageSizeComboBox.SelectedItem ?? 50);
                _currentPage = 1;
                
                // 更新分页信息和页码输入框的最大值
                UpdatePaginationInfo();
                
                // 确保当前页码在有效范围内
                if (_totalPages < 1)
                {
                    _currentPage = 1;
                    pageNumberInput.Maximum = 1;
                }
                else
                {
                    pageNumberInput.Maximum = _totalPages;
                    if (_currentPage > _totalPages)
                    {
                        _currentPage = _totalPages;
                    }
                }
                
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
            InitializeFilterControls();
        }

        private void InitializeFilterControls()
        {
            // 填充列名下拉框
            filterColumnComboBox.Items.Clear();
            if (_dataSource != null && _dataSource.Columns.Count > 0)
            {
                foreach (DataColumn column in _dataSource.Columns)
                {
                    filterColumnComboBox.Items.Add(column.ColumnName);
                }
                if (filterColumnComboBox.Items.Count > 0)
                {
                    filterColumnComboBox.SelectedIndex = 0;
                }
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void FilterColumnComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (filterColumnComboBox.SelectedItem != null)
            {
                string selectedColumn = filterColumnComboBox.SelectedItem.ToString()!;
                _currentFilterColumn = selectedColumn;

                // 填充值下拉框（获取该列的唯一值）
                filterValueComboBox.Items.Clear();
                if (_originalDataSource != null && _originalDataSource.Rows.Count > 0)
                {
                    DataView dv = new DataView(_originalDataSource);
                    DataTable distinctValues = dv.ToTable(true, selectedColumn);
                    foreach (DataRow row in distinctValues.Rows)
                    {
                        if (row[selectedColumn] != DBNull.Value)
                        {
                            // 使用null合并操作符确保不会传入null值
                            filterValueComboBox.Items.Add(row[selectedColumn].ToString() ?? string.Empty);
                        }
                    }
                    if (filterValueComboBox.Items.Count > 0)
                    {
                        filterValueComboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void ApplyFilterButton_Click(object? sender, EventArgs e)
        {
            if (filterColumnComboBox.SelectedItem != null && filterValueComboBox.SelectedItem != null)
            {
                _currentFilterColumn = filterColumnComboBox.SelectedItem.ToString();
                _currentFilterValue = filterValueComboBox.SelectedItem.ToString();

                // 应用筛选
                DataView dv = new DataView(_originalDataSource);
                // 使用!运算符表示_currentFilterColumn和_currentFilterValue不为null
                dv.RowFilter = $"[{_currentFilterColumn!}] = '{_currentFilterValue!}'";
                _dataSource = dv.ToTable();

                // 重置分页
                _currentPage = 1;
                LoadData();

                // 启用清除筛选按钮
                clearFilterButton.Enabled = true;
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求
        private void ClearFilterButton_Click(object? sender, EventArgs e)
        {
            // 清除筛选
            _dataSource = _originalDataSource.Copy();
            _currentFilterColumn = null;
            _currentFilterValue = null;

            // 重置分页
            _currentPage = 1;
            LoadData();

            // 禁用清除筛选按钮
            clearFilterButton.Enabled = false;
        }
        #endregion
    }
}