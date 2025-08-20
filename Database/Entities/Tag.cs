using System.Collections.Generic;

namespace DataVisualizationApp.Database.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ExcelFile> ExcelFiles { get; set; } = new List<ExcelFile>();
    }
}