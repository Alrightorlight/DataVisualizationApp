using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DataVisualizationApp.Services;
using DataVisualizationApp.Database.Repositories;
using DataVisualizationApp.Database.Entities;

namespace DataVisualizationApp
{
    public partial class MainForm : Form
    {
        // 存储导入的数据
        private DataTable _importedData = new DataTable();
        // Excel库服务
        private ExcelLibraryService _excelLibraryService;

        public MainForm()
        {
            // 初始化Excel库服务
            _excelLibraryService = new ExcelLibraryService();

            InitializeComponent();
            this.Text = "数据可视化分析系统";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 创建菜单栏
            CreateMenuStrip();

            // 创建工具栏
            CreateToolStrip();

            // 创建状态栏
            CreateStatusStrip();

            // 创建主面板
            CreateMainPanel();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region 菜单栏
        // 添加null!初始化以解决CS8618警告
private MenuStrip menuStrip = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem fileMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem importMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem exitMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem dataMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem viewDataMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem visualizeMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem helpMenuItem = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripMenuItem aboutMenuItem = null!;

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();

            // 文件菜单
            fileMenuItem = new ToolStripMenuItem("文件(&F)");
            importMenuItem = new ToolStripMenuItem("导入Excel(&I)", null, ImportExcel_Click);
            importMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            exitMenuItem = new ToolStripMenuItem("退出(&X)", null, Exit_Click);
            exitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;

            fileMenuItem.DropDownItems.Add(importMenuItem);
            fileMenuItem.DropDownItems.Add(new ToolStripSeparator());
            fileMenuItem.DropDownItems.Add(exitMenuItem);

            // 数据菜单
            dataMenuItem = new ToolStripMenuItem("数据(&D)");
            viewDataMenuItem = new ToolStripMenuItem("查看当前数据(&V)", null, ViewData_Click);
            viewDataMenuItem.ShortcutKeys = Keys.Control | Keys.D;
            visualizeMenuItem = new ToolStripMenuItem("数据可视化(&G)", null, Visualize_Click);
            visualizeMenuItem.ShortcutKeys = Keys.Control | Keys.G;
            var viewDatabaseDataMenuItem = new ToolStripMenuItem("查看数据库数据(&B)", null, ViewDatabaseData_Click);
            viewDatabaseDataMenuItem.ShortcutKeys = Keys.Control | Keys.B;

            dataMenuItem.DropDownItems.Add(viewDataMenuItem);
            dataMenuItem.DropDownItems.Add(viewDatabaseDataMenuItem);
            dataMenuItem.DropDownItems.Add(visualizeMenuItem);

            // 帮助菜单
            helpMenuItem = new ToolStripMenuItem("帮助(&H)");
            aboutMenuItem = new ToolStripMenuItem("关于(&A)", null, About_Click);
            helpMenuItem.DropDownItems.Add(aboutMenuItem);

            menuStrip.Items.Add(fileMenuItem);
            menuStrip.Items.Add(dataMenuItem);
            menuStrip.Items.Add(helpMenuItem);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }
        #endregion

        #region 工具栏
        // 添加null!初始化以解决CS8618警告
private ToolStrip toolStrip = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripButton importButton = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripButton viewDataButton = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripButton visualizeButton = null!;

        private void CreateToolStrip()
        {
            toolStrip = new ToolStrip();
            toolStrip.ImageScalingSize = new Size(24, 24);

            // 导入按钮
            importButton = new ToolStripButton("导入Excel");
            importButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            importButton.Click += ImportExcel_Click;

            // 查看数据按钮
            viewDataButton = new ToolStripButton("查看数据");
            viewDataButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            viewDataButton.Click += ViewData_Click;
            viewDataButton.Enabled = false; // 初始状态禁用

            // 可视化按钮
            visualizeButton = new ToolStripButton("数据可视化");
            visualizeButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            visualizeButton.Click += Visualize_Click;
            visualizeButton.Enabled = false; // 初始状态禁用

            // 查看数据库数据按钮
            var viewDatabaseButton = new ToolStripButton("查看数据库数据");
            viewDatabaseButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            viewDatabaseButton.Click += ViewDatabaseData_Click;
            viewDatabaseButton.Enabled = true; // 始终启用，因为可以查看历史数据

            toolStrip.Items.Add(importButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(viewDataButton);
            toolStrip.Items.Add(viewDatabaseButton);
            toolStrip.Items.Add(visualizeButton);

            this.Controls.Add(toolStrip);
        }
        #endregion

        #region 状态栏
        // 添加null!初始化以解决CS8618警告
private StatusStrip statusStrip = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripStatusLabel statusLabel = null!;
        // 添加null!初始化以解决CS8618警告
private ToolStripStatusLabel recordCountLabel = null!;

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();

            statusLabel = new ToolStripStatusLabel("就绪");
            statusLabel.Spring = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;

            recordCountLabel = new ToolStripStatusLabel("记录数: 0");
            recordCountLabel.TextAlign = ContentAlignment.MiddleRight;

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(recordCountLabel);

            this.Controls.Add(statusStrip);
        }
        #endregion

        #region 主面板
        // 添加null!初始化以解决CS8618警告
private Panel mainPanel = null!;
        // 添加null!初始化以解决CS8618警告
private Label welcomeLabel = null!;
        // 添加null!初始化以解决CS8618警告
private Button startImportButton = null!;

        private void CreateMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(240, 240, 240);

            // 欢迎标签
            welcomeLabel = new Label();
            welcomeLabel.Text = "欢迎使用数据可视化分析系统\n\n请点击\"导入Excel\"开始导入数据";
            welcomeLabel.Font = new Font("微软雅黑", 16, FontStyle.Regular);
            welcomeLabel.ForeColor = Color.FromArgb(64, 64, 64);
            welcomeLabel.TextAlign = ContentAlignment.MiddleCenter;
            welcomeLabel.AutoSize = false;
            welcomeLabel.Size = new Size(400, 120);
            welcomeLabel.Location = new Point(
            (mainPanel.Width - welcomeLabel.Width) / 2,
            (mainPanel.Height - welcomeLabel.Height) / 2 - 50
            );
            welcomeLabel.Anchor = AnchorStyles.None;

            // 开始导入按钮
            startImportButton = new Button();
            startImportButton.Text = "导入Excel文件";
            startImportButton.Font = new Font("微软雅黑", 12, FontStyle.Regular);
            startImportButton.Size = new Size(150, 40);
            startImportButton.Location = new Point(
            (mainPanel.Width - startImportButton.Width) / 2,
            welcomeLabel.Bottom + 20
            );
            startImportButton.Anchor = AnchorStyles.None;
            startImportButton.BackColor = Color.FromArgb(0, 120, 215);
            startImportButton.ForeColor = Color.White;
            startImportButton.FlatStyle = FlatStyle.Flat;
            startImportButton.FlatAppearance.BorderSize = 0;
            startImportButton.Click += ImportExcel_Click;

            mainPanel.Controls.Add(welcomeLabel);
            mainPanel.Controls.Add(startImportButton);
            mainPanel.Resize += MainPanel_Resize;

            this.Controls.Add(mainPanel);
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void MainPanel_Resize(object? sender, EventArgs e)
        {
            // 重新居中控件
            welcomeLabel.Location = new Point(
            (mainPanel.Width - welcomeLabel.Width) / 2,
            (mainPanel.Height - welcomeLabel.Height) / 2 - 50
            );
            startImportButton.Location = new Point(
            (mainPanel.Width - startImportButton.Width) / 2,
            welcomeLabel.Bottom + 20
            );
        }
        #endregion

        #region 事件处理
        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void ImportExcel_Click(object? sender, EventArgs e)
        {
            try
            {
                SetStatus("正在打开导入对话框...");

                using (var importForm = new DataVisualizationApp.Forms.DataImportForm())
                {
                    if (importForm.ShowDialog(this) == DialogResult.OK)
                    {
                        var importedData = importForm.ImportedData!;
                        // importedData已经通过!运算符确保非空，无需再次检查
if (importedData.Rows.Count > 0)
                        {
                            // 保存导入的数据
                            _importedData = importedData;

                            // 更新记录数显示
                            UpdateRecordCount(importedData.Rows.Count);

                            // 启用数据相关按钮
                            EnableDataButtons();

                            // 更新欢迎界面
                            UpdateWelcomePanel(importedData);

                            SetStatus($"成功导入 {importedData.Rows.Count} 行数据");

                            MessageBox.Show($"数据导入完成！\n行数: {importedData.Rows.Count}\n列数: {importedData.Columns.Count}",
                            "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        SetStatus("用户取消导入");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("导入失败");
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void ViewData_Click(object? sender, EventArgs e)
        {
            try
            {
                SetStatus("正在打开数据查看窗体...");

                // 创建并显示数据查看窗体
                using (var dataViewForm = new DataVisualizationApp.Forms.DataViewForm(_importedData))
                {
                    dataViewForm.ShowDialog(this);
                }

                SetStatus("就绪");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开数据查看失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("操作失败");
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void Visualize_Click(object? sender, EventArgs e)
        {
            try
            {
                SetStatus("正在打开可视化窗体...");

                // 创建并显示可视化窗体
                using (var visualizationForm = new DataVisualizationApp.Forms.VisualizationForm(_importedData))
                {
                    visualizationForm.ShowDialog(this);
                }

                SetStatus("就绪");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开可视化失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("操作失败");
            }
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void About_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("数据可视化分析系统 v1.0\n\n基于.NET 8.0和ScottPlot开发\n支持Excel数据导入和可视化分析",
            "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 将sender参数改为可空类型以匹配委托的可空性要求（CS8622警告）
private void Exit_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出程序吗？", "确认",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        #endregion

        // 查看数据库数据
        private void ViewDatabaseData_Click(object? sender, EventArgs e)
        {
            try
            {
                SetStatus("正在打开数据库数据查看窗体...");

                // 创建并显示数据库数据查看窗体
                using (var dbDataViewForm = new DataVisualizationApp.Forms.DatabaseDataViewForm(_excelLibraryService))
                {
                    dbDataViewForm.ShowDialog(this);
                }

                SetStatus("就绪");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开数据库数据查看失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("操作失败");
            }
        }

        #region 辅助方法
        ///

        /// 设置状态栏信息
        ///

        /// 状态信息
        public void SetStatus(string message)
        {
            statusLabel.Text = message;
            statusStrip.Update();
        }

        ///

        /// 更新记录数显示
        ///

        /// 记录数
        public void UpdateRecordCount(int count)
        {
            recordCountLabel.Text = $"记录数: {count:N0}";
        }

        ///

        /// 启用数据相关按钮
        ///

        public void EnableDataButtons()
        {
            viewDataButton.Enabled = true;
            visualizeButton.Enabled = true;
            viewDataMenuItem.Enabled = true;
            visualizeMenuItem.Enabled = true;
        }

        ///

        /// 禁用数据相关按钮
        ///

        public void DisableDataButtons()
        {
            viewDataButton.Enabled = false;
            visualizeButton.Enabled = false;
            viewDataMenuItem.Enabled = false;
            visualizeMenuItem.Enabled = false;
        }

        ///

        /// 更新欢迎面板显示导入成功信息
        ///

        /// 导入的数据
        private void UpdateWelcomePanel(DataTable data)
        {
            welcomeLabel.Text = $"数据导入成功！\n\n共导入 {data.Rows.Count:N0} 行数据，{data.Columns.Count} 列\n\n现在您可以查看数据或进行可视化分析";
            welcomeLabel.ForeColor = Color.FromArgb(0, 150, 0); // 绿色表示成功

            startImportButton.Text = "重新导入";
            startImportButton.BackColor = Color.FromArgb(100, 100, 100);
        }
        #endregion
    }
}
