using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DataVisualizationApp.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            // 设置EPPlus的许可证上下文（非商业使用）
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        ///

        /// 读取Excel文件基本信息
        ///

        /// 文件路径
        /// Excel文件信息
        public ExcelFileInfo GetExcelInfo(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在");

            var fileInfo = new FileInfo(filePath);
            var excelInfo = new ExcelFileInfo
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime
            };

            try
            {
                using (var package = new ExcelPackage(fileInfo))
                {
                    excelInfo.WorksheetNames = package.Workbook.Worksheets
                    .Select(ws => ws.Name).ToList();

                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        excelInfo.RowCount = worksheet.Dimension?.Rows ?? 0;
                        excelInfo.ColumnCount = worksheet.Dimension?.Columns ?? 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"读取Excel文件失败: {ex.Message}");
            }

            return excelInfo;
        }

        ///

        /// 预览Excel数据（前几行）
        ///

        /// 文件路径
        /// 工作表名称
        /// 预览行数
        /// 预览数据表
        public DataTable PreviewExcelData(string filePath, string? worksheetName = null, int previewRows = 10)
        {
            var fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = string.IsNullOrEmpty(worksheetName)
                ? package.Workbook.Worksheets[0]
                : package.Workbook.Worksheets[worksheetName];

                if (worksheet == null)
                    throw new ArgumentException("指定的工作表不存在");

                return ConvertWorksheetToDataTable(worksheet, previewRows, true);
            }
        }

        ///

        /// 读取完整Excel数据
        ///

        /// 文件路径
        /// 工作表名称
        /// 是否包含表头
        /// 表头行数
        /// 完整数据表
        public DataTable ReadExcelData(string filePath, string? worksheetName = null,
        bool hasHeaders = true, int headerRowCount = 1)
        {
            var fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = string.IsNullOrEmpty(worksheetName)
                ? package.Workbook.Worksheets[0]
                : package.Workbook.Worksheets[worksheetName];

                if (worksheet == null)
                    throw new ArgumentException("指定的工作表不存在");

                return ConvertWorksheetToDataTable(worksheet, -1, hasHeaders, headerRowCount);
            }
        }

        ///

        /// 将工作表转换为DataTable
        ///

        /// 工作表
        /// 最大行数（-1表示全部）
        /// 是否包含表头
        /// 表头行数
        /// DataTable
        private DataTable ConvertWorksheetToDataTable(ExcelWorksheet worksheet, int maxRows = -1,
        bool hasHeaders = true, int headerRowCount = 1)
        {
            var dataTable = new DataTable();

            if (worksheet.Dimension == null)
                return dataTable;

            int startRow = 1;
            int endRow = maxRows > 0 ? Math.Min(startRow + maxRows - 1, worksheet.Dimension.End.Row)
            : worksheet.Dimension.End.Row;
            int startCol = worksheet.Dimension.Start.Column;
            int endCol = worksheet.Dimension.End.Column;

            // 处理表头
            if (hasHeaders)
            {
                var headers = ProcessHeaders(worksheet, startCol, endCol, headerRowCount);
                foreach (var header in headers)
                {
                    dataTable.Columns.Add(header, typeof(object));
                }
                startRow += headerRowCount;
            }
            else
            {
                // 如果没有表头，创建默认列名
                for (int col = startCol; col <= endCol; col++)
                {
                    dataTable.Columns.Add($"Column{col}", typeof(object));
                }
            }

            // 处理数据行
            for (int row = startRow; row <= endRow; row++)
            {
                var dataRow = dataTable.NewRow();
                bool hasData = false;

                for (int col = startCol; col <= endCol; col++)
                {
                    var cellValue = GetCellValue(worksheet, row, col);
                    dataRow[col - startCol] = cellValue ?? DBNull.Value;
                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        hasData = true;
                }

                // 只添加非空行
                if (hasData)
                    dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        ///

        /// 处理表头（支持多行表头和合并单元格）
        ///

        /// 工作表
        /// 起始列
        /// 结束列
        /// 表头行数
        /// 表头列表
        private List<string> ProcessHeaders(ExcelWorksheet worksheet, int startCol, int endCol, int headerRowCount)
        {
            var headers = new List<string>();

            for (int col = startCol; col <= endCol; col++)
            {
                var headerParts = new List<string>();

                for (int row = 1; row <= headerRowCount; row++)
                {
                    var cellValue = GetCellValue(worksheet, row, col);
                    if (!string.IsNullOrWhiteSpace(cellValue?.ToString()))
                    {
                        headerParts.Add(cellValue?.ToString()?.Trim() ?? string.Empty);
                    }
                }

                // 合并多行表头
                var headerName = string.Join("_", headerParts.Where(h => !string.IsNullOrEmpty(h)));
                if (string.IsNullOrEmpty(headerName))
                {
                    headerName = $"Column{col}";
                }

                // 确保列名唯一
                string uniqueHeaderName = headerName;
                int counter = 1;
                while (headers.Contains(uniqueHeaderName))
                {
                    uniqueHeaderName = $"{headerName}_{counter}";
                    counter++;
                }

                headers.Add(uniqueHeaderName);
            }

            return headers;
        }

        ///

        /// 获取单元格值（处理合并单元格）
        ///

        /// 工作表
        /// 行号
        /// 列号
        /// 单元格值
        private object? GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            try
            {
                var cell = worksheet.Cells[row, col];

                // 检查是否是合并单元格
                if (cell.Merge)
                {
                    // 查找合并单元格的主单元格
                    foreach (var mergedCell in worksheet.MergedCells)
                    {
                        var range = worksheet.Cells[mergedCell];
                        if (range.Start.Row <= row && range.End.Row >= row &&
                        range.Start.Column <= col && range.End.Column >= col)
                        {
                            // 返回合并单元格区域左上角的值
                            return worksheet.Cells[range.Start.Row, range.Start.Column].Value;
                        }
                    }
                }

                // 处理不同类型的单元格值
                var value = cell.Value;
                if (value == null) return null;

                // 处理日期时间
                if (cell.Style.Numberformat.Format.Contains("yyyy") ||
                cell.Style.Numberformat.Format.Contains("mm") ||
                value is DateTime)
                {
                    if (double.TryParse(value.ToString(), out double dateValue))
                    {
                        return DateTime.FromOADate(dateValue);
                    }
                }

                return value;
            }
            catch
            {
                return null;
            }
        }

        ///

        /// 检测数据类型
        ///

        /// 数据表
        /// 列类型信息
        public Dictionary<string, Type> DetectColumnTypes(DataTable dataTable)
        {
            var columnTypes = new Dictionary<string, Type>();

            foreach (DataColumn column in dataTable.Columns)
            {
                var columnName = column.ColumnName;
                var detectedType = typeof(string); // 默认为字符串

                // 显式指定类型参数为 DataRow
                var nonNullValues = dataTable.Rows.Cast<DataRow>()
                    .Select(row => row[columnName])
                    .Where(value => value != null && value != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(value.ToString()))
                    .ToList();

                if (nonNullValues.Count > 0)
                {
                    // 检测整数
                    if (nonNullValues.All(v => int.TryParse(v.ToString(), out _)))
                    {
                        detectedType = typeof(int);
                    }
                    // 检测小数
                    else if (nonNullValues.All(v => double.TryParse(v.ToString(), out _)))
                    {
                        detectedType = typeof(double);
                    }
                    // 检测布尔值
                    else if (nonNullValues.All(v => bool.TryParse(v.ToString(), out _)))
                    {
                        detectedType = typeof(bool);
                    }
                    // 检测日期时间
                    else if (nonNullValues.All(v => DateTime.TryParse(v.ToString(), out _)))
                    {
                        detectedType = typeof(DateTime);
                    }
                }

                columnTypes[columnName] = detectedType;
            }

            return columnTypes;
        }
    }

    ///

    /// Excel文件信息
    ///

    public class ExcelFileInfo
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public List<string> WorksheetNames { get; set; } = new List<string>();
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
    }
}
