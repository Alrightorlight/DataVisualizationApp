using System;

namespace DataVisualizationApp.Database.Entities
{
    public class ExcelSheet
    {
        public int Id { get; set; }
        public int ExcelFileId { get; set; }
        public string SheetName { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public ExcelFile? ExcelFile { get; set; }
    }
}