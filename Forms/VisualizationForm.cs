using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataVisualizationApp.Services;
using ScottPlot.WinForms;

namespace DataVisualizationApp.Forms
{
    public class VisualizationForm : Form
    {
        private readonly DataBaseService dataBaseService = new DataBaseService();
        private ComboBox datasetComboBox;
        private ComboBox xColumnComboBox;
        private ComboBox yColumnComboBox;
        private ComboBox chartTypeComboBox;
        private Button plotButton;
        private FormsPlot formsPlot;

        public VisualizationForm()
        {
            Text = "数据可视化";
            Size = new Size(1100, 750);
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
            top.Controls.Add(new Label { Text = "数据集:", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            datasetComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
            datasetComboBox.SelectedIndexChanged += async (s, e) => await RefreshColumnSelectorsAsync();
            top.Controls.Add(datasetComboBox);

            top.Controls.Add(new Label { Text = "X列:", AutoSize = true, Margin = new Padding(16, 8, 8, 0) });
            xColumnComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            top.Controls.Add(xColumnComboBox);

            top.Controls.Add(new Label { Text = "Y列:", AutoSize = true, Margin = new Padding(16, 8, 8, 0) });
            yColumnComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            top.Controls.Add(yColumnComboBox);

            top.Controls.Add(new Label { Text = "图表类型:", AutoSize = true, Margin = new Padding(16, 8, 8, 0) });
            chartTypeComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            chartTypeComboBox.Items.AddRange(new[] { "折线图", "柱状图", "饼状图" });
            chartTypeComboBox.SelectedIndex = 0;
            top.Controls.Add(chartTypeComboBox);

            plotButton = new Button { Text = "绘制", Width = 80, Height = 28, Margin = new Padding(16, 6, 0, 0) };
            plotButton.Click += async (s, e) => await PlotAsync();
            top.Controls.Add(plotButton);

            formsPlot = new FormsPlot { Dock = DockStyle.Fill };

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(formsPlot, 0, 1);
            Controls.Add(layout);

            Shown += async (s, e) => await LoadDatasetsAsync();
        }

        private async Task LoadDatasetsAsync()
        {
            var datasets = await dataBaseService.GetDatasetsAsync();
            datasetComboBox.Items.Clear();
            foreach (var ds in datasets)
            {
                datasetComboBox.Items.Add(new ComboBoxItem(ds.Id, $"{ds.Name} (行:{ds.RowCount} 列:{ds.ColumnCount})"));
            }
            if (datasetComboBox.Items.Count > 0) datasetComboBox.SelectedIndex = 0;
        }

        private async Task RefreshColumnSelectorsAsync()
        {
            if (datasetComboBox.SelectedItem is ComboBoxItem item)
            {
                var table = await dataBaseService.GetDatasetTableAsync(item.Id);
                var columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                xColumnComboBox.Items.Clear();
                yColumnComboBox.Items.Clear();
                xColumnComboBox.Items.AddRange(columns);
                yColumnComboBox.Items.AddRange(columns);
                if (columns.Length > 0)
                {
                    xColumnComboBox.SelectedIndex = 0;
                    if (columns.Length > 1) yColumnComboBox.SelectedIndex = 1; else yColumnComboBox.SelectedIndex = 0;
                }
            }
        }

        private async Task PlotAsync()
        {
            if (datasetComboBox.SelectedItem is not ComboBoxItem item) return;
            if (xColumnComboBox.SelectedItem == null || yColumnComboBox.SelectedItem == null) return;
            if (chartTypeComboBox.SelectedItem == null) return;

            var table = await dataBaseService.GetDatasetTableAsync(item.Id);
            // 确保列名不为null
                string xCol = xColumnComboBox.SelectedItem?.ToString() ?? "X"; 
                xCol = string.IsNullOrEmpty(xCol) ? "X" : xCol; // 额外检查空字符串
                string yCol = yColumnComboBox.SelectedItem?.ToString() ?? "Y"; 
                yCol = string.IsNullOrEmpty(yCol) ? "Y" : yCol; // 额外检查空字符串
                string chartType = chartTypeComboBox.SelectedItem?.ToString() ?? "折线图"; // 添加null检查

            try
            {
                // 设置中文字体 (ScottPlot 5.0.55)
                // 使用字符串参数直接设置字体
                formsPlot.Plot.Font.Set("SimHei");

                formsPlot.Plot.Clear();

                if (chartType == "饼状图")
                {
                    // 饼状图实现
                    var categories = table.AsEnumerable()
                        .Select(r => r[xCol]?.ToString() ?? "")
                        .Where(v => !string.IsNullOrEmpty(v))
                        .ToArray();

                    var values = table.AsEnumerable()
                        .Select(r => r[yCol] ?? DBNull.Value)
                        .Where(v => v != null && v != DBNull.Value)
                        .Select(v => Convert.ToDouble(v))
                        .ToArray();

                    if (categories.Length > 0 && values.Length > 0)
                    {
                        int n = Math.Min(categories.Length, values.Length);
                        categories = categories.Take(n).ToArray();
                        values = values.Take(n).ToArray();

                        // 创建饼图并添加到图表
                        // 创建饼图并添加到图表 (ScottPlot 5.0.55)
                        // 创建饼图并添加标签 (ScottPlot 5.0.55)
                        // 使用元组数组创建饼图
                        var pieData = values.Zip(categories, (v, l) => new { Value = v, Label = l }).ToList();
                        formsPlot.Plot.Add.Pie(pieData.Select(p => p.Value).ToArray());
                        // 设置饼图标签
                        formsPlot.Plot.Legend.IsVisible = true;
                        formsPlot.Plot.Title($"{yCol} 饼状图");
                    }
                }
                else
                {
                    // 折线图和柱状图共用的数据处理
                    var xValues = table.AsEnumerable()
                        .Select(r => r[xCol] ?? DBNull.Value)
                        .Where(v => v != null && v != DBNull.Value)
                        .Select(v =>
                        {
                            if (DateTime.TryParse(v.ToString(), out DateTime dateValue))
                                return dateValue.ToOADate();
                            else if (double.TryParse(v.ToString(), out double doubleValue))
                                return doubleValue;
                            return 0.0;
                        })
                        .ToArray();

                    var yValues = table.AsEnumerable()
                        .Select(r => r[yCol] ?? DBNull.Value)
                        .Where(v => v != null && v != DBNull.Value)
                        .Select(v => Convert.ToDouble(v))
                        .ToArray();

                    int n = Math.Min(xValues.Length, yValues.Length);
                    xValues = xValues.Take(n).ToArray();
                    yValues = yValues.Take(n).ToArray();

                    if (chartType == "折线图")
                    {
                        formsPlot.Plot.Add.Scatter(xValues, yValues);
                    }
                    else if (chartType == "柱状图")
                    {
                        // 尝试使用不同的柱状图API
                            // 创建柱状图并添加到图表
                            // 创建柱状图并添加到图表 (ScottPlot 5.0.55)
                            formsPlot.Plot.Add.Bars(yValues);
                        // 为柱状图设置X轴标签
                        // 设置X轴标签
                        // 移除SetAxisLabels，已通过XLabel和YLabel设置;
                        // 使用正确的方法设置X轴刻度
                        // 使用替代方法设置X轴刻度
                        // 移除设置X轴刻度的代码，使用默认刻度
                    }

                    formsPlot.Plot.Title($"{yCol} vs {xCol}");
                    formsPlot.Plot.XLabel(xCol ?? "X"); // 添加null检查
                    formsPlot.Plot.YLabel(yCol ?? "Y"); // 添加null检查
                }

                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘图失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem
        {
            public int Id { get; }
            public string Text { get; }
            public ComboBoxItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }
    }
}


