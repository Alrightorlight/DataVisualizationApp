using System;
using System.Collections.Generic;

namespace DataVisualizationApp.Database.Entities
{
    public class ExcelFile
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime LastModified { get; set; }
        public string Category { get; set; } = "General";
        public string? Description { get; set; }
        public DateTime? LastAccessed { get; set; }
        public List<ExcelSheet> Sheets { get; set; } = new List<ExcelSheet>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}