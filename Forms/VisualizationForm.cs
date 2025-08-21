using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ScottPlot;
using ScottPlot.WinForms;
using System.Collections.Generic;

namespace DataVisualizationApp.Forms
{
    public partial class VisualizationForm : Form
    {
        // 数据源
        private DataTable _dataSource = null!;
        // 当前选中的X轴列
        private string _selectedXColumn = string.Empty;
        // 当前选中的Y轴列
        private string _selectedYColumn = string.Empty;
        // 当前图表类型
        private ChartType _currentChartType = ChartType.Line;
        
        // 排序和筛选变量
        private int _filterCount = 5;
        private SortOrder _currentSortOrder = SortOrder.Descending;

        public enum SortOrder
        {
            Ascending,  // 从小到大
            Descending  // 从大到小
        }

        // 图表类型枚举
        public enum ChartType
        {
            Line,
            Bar,
            Pie
        }

        public VisualizationForm(DataTable dataSource)
        {
            if (dataSource == null || dataSource.Rows.Count == 0)
                throw new ArgumentNullException(nameof(dataSource), "数据源不能为空");

            _dataSource = dataSource;
            InitializeComponent();
            this.Text = "数据可视化";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 初始化图表控件
            InitializeChartControl();
            // 加载列到下拉框
            LoadColumnsToComboBox();
            // 初始绘制图表
            DrawChart();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 创建主容器
            TableLayoutPanel mainContainer = new TableLayoutPanel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.ColumnCount = 1;
            mainContainer.RowCount = 2;
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // 控件面板
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 图表面板

            // 创建控件面板
            CreateControlPanel();
            mainContainer.Controls.Add(controlPanel, 0, 0);

            // 创建图表面板
            CreateChartPanel();
            mainContainer.Controls.Add(chartPanel, 0, 1);

            this.Controls.Add(mainContainer);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region 控件面板
        private Panel controlPanel = null!;
        private Label xColumnLabel = null!;
        private ComboBox xColumnComboBox = null!;
        private Label yColumnLabel = null!;
        private ComboBox yColumnComboBox = null!;
        private Label chartTypeLabel = null!;
        private ComboBox chartTypeComboBox = null!;
        private Button refreshButton = null!;
        private Button settingsButton = null!;

        // 排序控件
        private Label sortOrderLabel = null!;
        private ComboBox sortOrderComboBox = null!;

        // 数量筛选控件
        private Label filterCountLabel = null!;
        private ComboBox filterCountComboBox = null!;

        private void CreateControlPanel()
        {
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.BackColor = Color.FromArgb(224, 224, 224);
            controlPanel.Padding = new Padding(10);

            // X轴列标签
            xColumnLabel = new Label();
            xColumnLabel.Text = "X轴列: ";
            xColumnLabel.Location = new Point(10, 20);
            xColumnLabel.AutoSize = true;

            // X轴列下拉框
            xColumnComboBox = new ComboBox();
            xColumnComboBox.Size = new Size(300, 23);  // 增加宽度以显示长表头
            xColumnComboBox.Location = new Point(xColumnLabel.Right - 40, 17);
            xColumnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            xColumnComboBox.SelectedIndexChanged += XColumnComboBox_SelectedIndexChanged;

            // Y轴列标签
            yColumnLabel = new Label();
            yColumnLabel.Text = "Y轴列: ";
            yColumnLabel.Location = new Point(xColumnComboBox.Right + 20, 20);
            yColumnLabel.AutoSize = true;

            // Y轴列下拉框
            yColumnComboBox = new ComboBox();
            yColumnComboBox.Size = new Size(300, 23);
            yColumnComboBox.Location = new Point(yColumnLabel.Right - 40, 17);
            yColumnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            yColumnComboBox.SelectedIndexChanged += YColumnComboBox_SelectedIndexChanged;

            // 图表类型标签
            chartTypeLabel = new Label();
            chartTypeLabel.Text = "图表类型: ";
            chartTypeLabel.Location = new Point(yColumnComboBox.Right + 20, 20);
            chartTypeLabel.AutoSize = true;

            // 图表类型下拉框
            chartTypeComboBox = new ComboBox();
            chartTypeComboBox.Size = new Size(120, 23);
            chartTypeComboBox.Location = new Point(chartTypeLabel.Right - 20, 17);
            chartTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            chartTypeComboBox.Items.AddRange(new object[] { "折线图", "柱状图", "饼状图" });
            chartTypeComboBox.SelectedIndex = 0; // 默认折线图
            chartTypeComboBox.SelectedIndexChanged += ChartTypeComboBox_SelectedIndexChanged;

            // 刷新按钮
            refreshButton = new Button();
            refreshButton.Text = "刷新图表";
            refreshButton.Size = new Size(90, 25);
            refreshButton.Location = new Point(chartTypeComboBox.Right + 20, 15);
            refreshButton.Click += RefreshButton_Click;

            // 设置按钮
            settingsButton = new Button();
            settingsButton.Text = "图表设置";
            settingsButton.Size = new Size(90, 25);
            settingsButton.Location = new Point(refreshButton.Right + 10, 15);
            settingsButton.Click += SettingsButton_Click;

            // 数量标签
            filterCountLabel = new Label();
            filterCountLabel.Text = "显示数量: ";
            filterCountLabel.Location = new Point(settingsButton.Right + 20, 20);
            filterCountLabel.AutoSize = true;

            // 数量下拉框
            filterCountComboBox = new ComboBox();
            filterCountComboBox.Size = new Size(60, 23);
            filterCountComboBox.Location = new Point(filterCountLabel.Right - 20, 17);
            filterCountComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            filterCountComboBox.Items.AddRange(new object[] { "5", "10", "20", "50", "100", "全部" });
            filterCountComboBox.SelectedIndex = 0;
            filterCountComboBox.SelectedIndexChanged += FilterCountComboBox_SelectedIndexChanged;

            // 排序方向标签
            sortOrderLabel = new Label();
            sortOrderLabel.Text = "排序: ";
            sortOrderLabel.Location = new Point(filterCountComboBox.Right + 20, 20);
            sortOrderLabel.AutoSize = true;

            // 排序方向下拉框
            sortOrderComboBox = new ComboBox();
            sortOrderComboBox.Size = new Size(100, 23);
            sortOrderComboBox.Location = new Point(sortOrderLabel.Right - 40, 17);
            sortOrderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sortOrderComboBox.Items.AddRange(new object[] { "从大到小", "从小到大" });
            sortOrderComboBox.SelectedIndex = 0; // 默认从大到小
            sortOrderComboBox.SelectedIndexChanged += SortOrderComboBox_SelectedIndexChanged;

            // 添加数量筛选控件到面板
            controlPanel.Controls.Add(filterCountLabel);
            controlPanel.Controls.Add(filterCountComboBox);

            // 添加控件到面板
            controlPanel.Controls.Add(xColumnLabel);
            controlPanel.Controls.Add(xColumnComboBox);
            controlPanel.Controls.Add(yColumnLabel);
            controlPanel.Controls.Add(yColumnComboBox);
            controlPanel.Controls.Add(chartTypeLabel);
            controlPanel.Controls.Add(chartTypeComboBox);
            controlPanel.Controls.Add(refreshButton);
            controlPanel.Controls.Add(settingsButton);
            controlPanel.Controls.Add(sortOrderLabel);
            controlPanel.Controls.Add(sortOrderComboBox);
        }
        #endregion

        #region 图表面板
        private Panel chartPanel = null!;
        private FormsPlot formsPlot = null!;

        private void CreateChartPanel()
        {
            chartPanel = new Panel();
            chartPanel.Dock = DockStyle.Fill;
            chartPanel.BackColor = Color.White;

            // 创建ScottPlot控件
            formsPlot = new FormsPlot();
            formsPlot.Dock = DockStyle.Fill;
            chartPanel.Controls.Add(formsPlot);
        }

        private void InitializeChartControl()
        {
            // 配置图表初始设置
            formsPlot.Plot.Title("数据可视化图表");
            formsPlot.Plot.XLabel("X轴");
            formsPlot.Plot.YLabel("Y轴");
            formsPlot.Plot.Legend();
        }
        #endregion

        #region 辅助方法
        private void LoadColumnsToComboBox()
        {
            // 清空下拉框
            xColumnComboBox.Items.Clear();
            yColumnComboBox.Items.Clear();

            // 添加列到下拉框
            foreach (DataColumn column in _dataSource.Columns)
            {
                xColumnComboBox.Items.Add(column.ColumnName);
                yColumnComboBox.Items.Add(column.ColumnName);
            }

            // 默认选中第一列
            if (xColumnComboBox.Items.Count > 0)
            {
                xColumnComboBox.SelectedIndex = 0;
                _selectedXColumn = xColumnComboBox.Items[0]?.ToString() ?? string.Empty;
            }

            if (yColumnComboBox.Items.Count > 1)
            {
                yColumnComboBox.SelectedIndex = 1;
                _selectedYColumn = yColumnComboBox.Items[1]?.ToString() ?? string.Empty;
            }
            else if (yColumnComboBox.Items.Count > 0)
            {
                yColumnComboBox.SelectedIndex = 0;
                _selectedYColumn = yColumnComboBox.Items[0]?.ToString() ?? string.Empty;
            }
        }

        private void DrawChart()
        {
            // 清除现有图表
            formsPlot.Plot.Clear();

            // 根据选中的图表类型绘制图表
            switch (_currentChartType)
            {
                case ChartType.Line:
                case ChartType.Bar:
                    // 检查数据有效性
                    double[] yValues = GetDoubleValues(_selectedYColumn);
                    if (yValues.Length == 0)
                    {
                        formsPlot.Plot.Title("错误：Y轴数据无效");
                        formsPlot.Plot.XLabel("");
                        formsPlot.Plot.YLabel("");
                        formsPlot.Plot.AddText("所选Y轴列没有可转换为数值的数据\n请选择其他列或检查数据格式", 0.5, 0.5);
                        formsPlot.Plot.SetAxisLimits(0, 1, 0, 1);
                        formsPlot.Refresh();
                        return;
                    }

                    if (_currentChartType == ChartType.Line)
                        DrawLineChart();
                    else
                        DrawBarChart();
                    break;
                case ChartType.Pie:
                    // 检查X轴数据有效性
                    string[] xValues = GetStringValues(_selectedXColumn);
                    if (xValues.Length == 0)
                    {
                        formsPlot.Plot.Title("错误：X轴数据无效");
                        formsPlot.Plot.XLabel("");
                        formsPlot.Plot.YLabel("");
                        formsPlot.Plot.AddText("所选X轴列没有有效数据\n请选择其他列或检查数据格式", 0.5, 0.5);
                        formsPlot.Plot.SetAxisLimits(0, 1, 0, 1);
                        formsPlot.Refresh();
                        return;
                    }
                    DrawPieChart();
                    break;
            }

            // 更新图表
            formsPlot.Refresh();
        }

        private void DrawLineChart()
        {
            // 获取X和Y轴数据
            string[] xLabels = GetStringValues(_selectedXColumn);
            double[] yValues = GetDoubleValues(_selectedYColumn);

            // 应用筛选
            var (filteredXLabels, filteredYValues) = FilterData(xLabels, yValues);

            // 准备X轴数值
            double[] xValues = Enumerable.Range(0, filteredXLabels.Length).Select(i => (double)i).ToArray();

            // 添加折线图
            // 添加折线图并启用数据点标记
            var scatterPlot = formsPlot.Plot.AddScatter(xValues, filteredYValues, label: $"{_selectedYColumn} vs {_selectedXColumn}");
            scatterPlot.MarkerSize = 6;  // 设置标记大小
            // 注释掉标记形状设置，使用默认值
            // scatterPlot.MarkerShape = MarkerShape.Circle;
            scatterPlot.LineWidth = 2;  // 设置线宽

            // 添加数据标签
            for (int i = 0; i < xValues.Length; i++)
            {
                // 调整标签位置，使其位于数据点正上方且靠近
                double labelOffset = Math.Max(filteredYValues.Max() * 0.02, 0.5); // 确保标签在点上方足够明显
                double labelYPosition = filteredYValues[i] + labelOffset;
                // 确保标签不会高于图表顶部
                labelYPosition = Math.Min(labelYPosition, formsPlot.Plot.GetAxisLimits().YMax);
                formsPlot.Plot.AddText(filteredYValues[i].ToString("F2"), xValues[i], labelYPosition,
                    color: Color.Black, size: 10);
            }

            // 设置X轴标签
            formsPlot.Plot.XTicks(xValues, filteredXLabels);

            // 设置图表标题和轴标签
            string filterInfo = GetFilterInfo();
            formsPlot.Plot.Title($"折线图: {_selectedYColumn} vs {_selectedXColumn} {filterInfo}");
            formsPlot.Plot.XLabel(_selectedXColumn);
            formsPlot.Plot.YLabel(_selectedYColumn);

            // 添加网格
            formsPlot.Plot.Grid(enable: true);

            // 确保Y轴标签可见
            // 移除ManualTickSpacing调用，让ScottPlot自动处理Y轴刻度
            formsPlot.Plot.YAxis2.IsVisible = false;  // 隐藏右侧Y轴
        }

        private string GetFilterInfo()
        {
            string countText = _filterCount >= int.MaxValue / 2 ? "全部" : _filterCount.ToString();
            string sortOrder = _currentSortOrder == SortOrder.Descending ? "从大到小" : "从小到大";
            return $"(显示{countText}个，{sortOrder})";
        }

        private void DrawBarChart()
        {
            // 获取X和Y轴数据
            string[] xLabels = GetStringValues(_selectedXColumn);
            double[] yValues = GetDoubleValues(_selectedYColumn);

            // 应用筛选
            var (filteredXLabels, filteredYValues) = FilterData(xLabels, yValues);

            // 添加柱状图
            double[] xPositions = Enumerable.Range(0, filteredXLabels.Length).Select(i => (double)i).ToArray();
            // 添加柱状图并设置宽度
            var barPlot = formsPlot.Plot.AddBar(filteredYValues, xPositions);
            barPlot.BarWidth = 0.4;  // 减小柱状图宽度

            // 为柱状图添加数据标签
            for (int i = 0; i < filteredYValues.Length; i++)
            {
                formsPlot.Plot.AddText(filteredYValues[i].ToString("F2"), xPositions[i], filteredYValues[i] + filteredYValues.Max() * 0.02,
                    color: Color.Black, size: 10);  // 移除不支持的bold参数
            }
            formsPlot.Plot.XTicks(xPositions, filteredXLabels);

            // 确保Y轴标签可见
            // 移除ManualTickSpacing调用，让ScottPlot自动处理Y轴刻度
            formsPlot.Plot.YAxis2.IsVisible = false;  // 隐藏右侧Y轴

            // 设置图表标题和轴标签
            string filterInfo = GetFilterInfo();
            formsPlot.Plot.Title($"柱状图: {_selectedYColumn} by {_selectedXColumn} {filterInfo}");
            formsPlot.Plot.XLabel(_selectedXColumn);
            formsPlot.Plot.YLabel(_selectedYColumn);

            // 添加网格
            formsPlot.Plot.Grid(enable: true);
            formsPlot.Plot.XAxis.Grid(false);

            // 旋转X轴标签以避免重叠
            formsPlot.Plot.XAxis.TickLabelStyle(rotation: 45);
        }

        private void DrawPieChart()
        {
            // 获取X轴列的文本值
            string[] xValues = GetStringValues(_selectedXColumn);

            // 统计每个唯一值出现的次数
            var valueCounts = xValues.GroupBy(x => x)
                                     .ToDictionary(g => g.Key, g => g.Count());

            // 提取标签和对应的值
            string[] labels = valueCounts.Keys.ToArray();
            double[] values = valueCounts.Values.Select(c => (double)c).ToArray();

            // 检查是否有数据
            if (values.Length == 0 || values.Sum() == 0)
            {
                formsPlot.Plot.Title("错误：没有可用数据");
                formsPlot.Plot.AddText("无法生成饼图，所选列没有有效数据", 0.5, 0.5);
                formsPlot.Plot.SetAxisLimits(0, 1, 0, 1);
                return;
            }

            // 计算总值用于百分比计算
            double total = values.Sum();

            // 创建带数量和百分比的标签
            string[] labelsWithDetails = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                double percent = (values[i] / total) * 100;
                labelsWithDetails[i] = $"{labels[i]}: {valueCounts[labels[i]]}个 ({percent:F1}%)";
            }

            // 添加饼图
            var pie = formsPlot.Plot.AddPie(values);
            pie.SliceLabels = labelsWithDetails;
            pie.ShowLabels = true;  // 显示切片标签

            // 开启图例
            formsPlot.Plot.Legend(enable: true);

            // 设置图表标题
            formsPlot.Plot.Title($"饼状图: {_selectedXColumn}分布");

            // 饼图不需要轴标签
            formsPlot.Plot.XLabel("");
            formsPlot.Plot.YLabel("");
        }

        private double[] GetDoubleValues(string columnName)
        {
            // 尝试将列数据转换为double
            var values = new List<double>();
            foreach (DataRow row in _dataSource.Rows)
            {
                if (double.TryParse(row[columnName].ToString(), out double value))
                {
                    values.Add(value);
                }
            }
            return values.ToArray();
        }

        private (string[] xLabels, double[] yValues) FilterData(string[] xLabels, double[] yValues)
        {
            // 如果没有数据，直接返回
            if (yValues.Length == 0)
            {
                return (xLabels, yValues);
            }

            // 创建数据对列表
            var dataPairs = new List<(string xLabel, double yValue)>();
            for (int i = 0; i < Math.Min(xLabels.Length, yValues.Length); i++)
            {
                dataPairs.Add((xLabels[i], yValues[i]));
            }

            // 根据排序方向排序
            if (_currentSortOrder == SortOrder.Descending)
            {
                // 降序排序：从大到小，左高右低
                dataPairs.Sort((a, b) => b.yValue.CompareTo(a.yValue));
            }
            else
            {
                // 升序排序：从小到大，左低右高
                dataPairs.Sort((a, b) => a.yValue.CompareTo(b.yValue));
            }

            // 应用数量筛选
            int takeCount = _filterCount;
            if (takeCount >= int.MaxValue / 2 || takeCount > dataPairs.Count)
            {
                takeCount = dataPairs.Count;
            }

            // 取前N个数据（根据排序方向，已经是正确的顺序）
            var filteredData = dataPairs.Take(takeCount).ToList();

            // 提取筛选后的标签和值
            string[] filteredXLabels = filteredData.Select(pair => pair.xLabel).ToArray();
            double[] filteredYValues = filteredData.Select(pair => pair.yValue).ToArray();

            return (filteredXLabels, filteredYValues);
        }

        private string[] GetStringValues(string columnName)
        {
            // 获取列的字符串值
            var values = new List<string>();
            foreach (DataRow row in _dataSource.Rows)
            {
                values.Add(row[columnName].ToString()!);
            }
            return values.ToArray();
        }
        #endregion

        #region 事件处理
        private void XColumnComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (xColumnComboBox.SelectedItem != null)
            {
                _selectedXColumn = xColumnComboBox.SelectedItem.ToString()!;
            }
        }

        private void YColumnComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (yColumnComboBox.SelectedItem != null)
            {
                _selectedYColumn = yColumnComboBox.SelectedItem.ToString()!;
            }
        }

        private void ChartTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (chartTypeComboBox.SelectedIndex == 0)
            {
                _currentChartType = ChartType.Line;
                // 显示所有轴控件
                xColumnLabel.Visible = true;
                xColumnComboBox.Visible = true;
                yColumnLabel.Visible = true;
                yColumnComboBox.Visible = true;
                // 恢复控件位置
                xColumnLabel.Location = new Point(10, 20);
                xColumnComboBox.Location = new Point(xColumnLabel.Right + 5, 17);
                yColumnLabel.Location = new Point(xColumnComboBox.Right + 20, 20);
                yColumnComboBox.Location = new Point(yColumnLabel.Right + 5, 17);
                chartTypeLabel.Location = new Point(yColumnComboBox.Right + 20, 20);
                chartTypeComboBox.Location = new Point(chartTypeLabel.Right + 5, 17);
                refreshButton.Location = new Point(chartTypeComboBox.Right + 20, 15);
                settingsButton.Location = new Point(refreshButton.Right + 10, 15);
            }
            else if (chartTypeComboBox.SelectedIndex == 1)
            {
                _currentChartType = ChartType.Bar;
                // 显示所有轴控件
                xColumnLabel.Visible = true;
                xColumnComboBox.Visible = true;
                yColumnLabel.Visible = true;
                yColumnComboBox.Visible = true;
                // 恢复控件位置
                xColumnLabel.Location = new Point(10, 20);
                xColumnComboBox.Location = new Point(xColumnLabel.Right + 5, 17);
                yColumnLabel.Location = new Point(xColumnComboBox.Right + 20, 20);
                yColumnComboBox.Location = new Point(yColumnLabel.Right + 5, 17);
                chartTypeLabel.Location = new Point(yColumnComboBox.Right + 20, 20);
                chartTypeComboBox.Location = new Point(chartTypeLabel.Right + 5, 17);
                refreshButton.Location = new Point(chartTypeComboBox.Right + 20, 15);
                settingsButton.Location = new Point(refreshButton.Right + 10, 15);
            }
            else if (chartTypeComboBox.SelectedIndex == 2)
            {
                _currentChartType = ChartType.Pie;
                // 隐藏Y轴控件，只保留X轴控件
                xColumnLabel.Visible = true;
                xColumnComboBox.Visible = true;
                yColumnLabel.Visible = false;
                yColumnComboBox.Visible = false;
                // 调整其他控件位置
                xColumnLabel.Location = new Point(10, 20);
                xColumnComboBox.Location = new Point(xColumnLabel.Right + 5, 17);
                chartTypeLabel.Location = new Point(xColumnComboBox.Right + 20, 20);
                chartTypeComboBox.Location = new Point(chartTypeLabel.Right + 5, 17);
                refreshButton.Location = new Point(chartTypeComboBox.Right + 20, 15);
                settingsButton.Location = new Point(refreshButton.Right + 10, 15);
            }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            DrawChart();
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            // 这里可以打开图表设置对话框
            MessageBox.Show("图表设置功能将在后续实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FilterCountComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (filterCountComboBox.SelectedItem?.ToString() == "全部")
                _filterCount = int.MaxValue;  // 使用int.MaxValue表示全部
            else
                int.TryParse(filterCountComboBox.SelectedItem?.ToString(), out _filterCount);
            
            // 数量改变后立即更新图表
            DrawChart();
        }

        private void SortOrderComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _currentSortOrder = sortOrderComboBox.SelectedIndex == 0 ? SortOrder.Descending : SortOrder.Ascending;
            // 排序改变后立即更新图表
            DrawChart();
        }
        #endregion
    }
}