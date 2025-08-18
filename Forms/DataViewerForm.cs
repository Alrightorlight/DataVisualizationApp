using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataVisualizationApp.Services;

namespace DataVisualizationApp.Forms
{
    public class DataViewerForm : Form
    {
        private readonly DataBaseService dataBaseService = new DataBaseService();
        private ComboBox datasetComboBox;
        private DataGridView grid;
        private Label infoLabel;

        public DataViewerForm()
        {
            Text = "数据查看";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
            topPanel.Controls.Add(new Label { Text = "数据集:", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            datasetComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            datasetComboBox.SelectedIndexChanged += async (s, e) => await LoadSelectedDatasetAsync();
            topPanel.Controls.Add(datasetComboBox);
            infoLabel = new Label { AutoSize = true, ForeColor = Color.Gray, Margin = new Padding(16, 8, 0, 0) };
            topPanel.Controls.Add(infoLabel);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(245, 245, 245) }
            };

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            Controls.Add(layout);

            Shown += async (s, e) => await LoadDatasetsAsync();
        }

        private async Task LoadDatasetsAsync()
        {
            var datasets = await dataBaseService.GetDatasetsAsync();
            datasetComboBox.Items.Clear();
            foreach (var ds in datasets)
            {
                datasetComboBox.Items.Add(new ComboBoxItem(ds.Id, $"{ds.Name}  (行:{ds.RowCount}, 列:{ds.ColumnCount}, 时间:{ds.ImportedAt:yyyy-MM-dd HH:mm})"));
            }
            if (datasetComboBox.Items.Count > 0) datasetComboBox.SelectedIndex = 0;
            infoLabel.Text = datasets.Length == 0 ? "暂无已保存数据" : "";
        }

        private async Task LoadSelectedDatasetAsync()
        {
            if (datasetComboBox.SelectedItem is ComboBoxItem item)
            {
                var table = await dataBaseService.GetDatasetTableAsync(item.Id);
                grid.DataSource = table;
                infoLabel.Text = $"已加载: 行 {table.Rows.Count}, 列 {table.Columns.Count}";
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


