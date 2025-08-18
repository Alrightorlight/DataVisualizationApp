using System;

namespace DataVisualizationApp.Models
{
	public class ImportedDataset
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
		public int RowCount { get; set; }
		public int ColumnCount { get; set; }
		public string DataJson { get; set; } = string.Empty;
	}
}



