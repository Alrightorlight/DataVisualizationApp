using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataVisualizationApp.Forms
{
    public partial class DataViewForm : Form
    {
        // 数据源
        private DataTable _dataSource;
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
        private Panel mainPanel;
        private DataGridView dataGridView;
        private Label pageInfoLabel;

        private void CreateMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);
            mainPanel.BackColor = Color.FromArgb(240, 240, 240);

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

            // 添加到面板
            mainPanel.Controls.Add(dataGridView);
            this.Controls.Add(mainPanel);
        }
        #endregion

        #region 分页控件
        private Panel paginationPanel;
        private Button firstPageButton;
        private Button prevPageButton;
        private Button nextPageButton;
        private Button lastPageButton;
        private NumericUpDown pageNumberInput;
        private Label pageSeparatorLabel;
        private Label totalPagesLabel;
        private ComboBox pageSizeComboBox;
        private Label pageSizeLabel;

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

        private void FirstPageButton_Click(object sender, EventArgs e)
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        private void PrevPageButton_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        private void LastPageButton_Click(object sender, EventArgs e)
        {
            if (_currentPage != _totalPages)
            {
                _currentPage = _totalPages;
                pageNumberInput.Value = _currentPage;
                LoadData();
            }
        }

        private void PageNumberInput_ValueChanged(object sender, EventArgs e)
        {
            int newPage = (int)pageNumberInput.Value;
            if (newPage != _currentPage && newPage >= 1 && newPage <= _totalPages)
            {
                _currentPage = newPage;
                LoadData();
            }
        }

        private void PageSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pageSizeComboBox.SelectedItem != null)
            {
                _pageSize = (int)pageSizeComboBox.SelectedItem;
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
        }
        #endregion
    }
}