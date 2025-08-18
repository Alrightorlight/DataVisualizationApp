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

            var table = await dataBaseService.GetDatasetTableAsync(item.Id);
            string xCol = xColumnComboBox.SelectedItem.ToString();
            string yCol = yColumnComboBox.SelectedItem.ToString();

            try
            {
                var xValues = table.AsEnumerable()
                    .Select(r => r[xCol])
                    .Where(v => v != null && v != DBNull.Value)
                    .Select(v => Convert.ToDouble(v))
                    .ToArray();

                var yValues = table.AsEnumerable()
                    .Select(r => r[yCol])
                    .Where(v => v != null && v != DBNull.Value)
                    .Select(v => Convert.ToDouble(v))
                    .ToArray();

                int n = Math.Min(xValues.Length, yValues.Length);
                xValues = xValues.Take(n).ToArray();
                yValues = yValues.Take(n).ToArray();

                formsPlot.Plot.Clear();
                formsPlot.Plot.Add.Scatter(xValues, yValues);
                formsPlot.Plot.Title($"{yCol} vs {xCol}");
                formsPlot.Plot.XLabel(xCol);
                formsPlot.Plot.YLabel(yCol);
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


