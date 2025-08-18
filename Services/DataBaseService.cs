using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataVisualizationApp.DataBase;
using DataVisualizationApp.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataVisualizationApp.Services
{
    public class DataBaseService
    {
        public async Task<ImportedDataset> SaveDatasetAsync(string name, DataTable dataTable)
        {
            if (string.IsNullOrWhiteSpace(name)) name = $"Dataset_{DateTime.Now:yyyyMMdd_HHmmss}";

            var dataset = new ImportedDataset
            {
                Name = name,
                ImportedAt = DateTime.UtcNow,
                RowCount = dataTable?.Rows.Count ?? 0,
                ColumnCount = dataTable?.Columns.Count ?? 0,
                DataJson = SerializeDataTable(dataTable)
            };

            using (var db = new AppDbContext())
            {
                await db.Database.EnsureCreatedAsync();
                db.ImportedDatasets.Add(dataset);
                await db.SaveChangesAsync();
            }

            return dataset;
        }

        public async Task<ImportedDataset[]> GetDatasetsAsync()
        {
            using (var db = new AppDbContext())
            {
                await db.Database.EnsureCreatedAsync();
                return await db.ImportedDatasets
                    .OrderByDescending(d => d.ImportedAt)
                    .ToArrayAsync();
            }
        }

        public async Task<DataTable> GetDatasetTableAsync(int id)
        {
            using (var db = new AppDbContext())
            {
                await db.Database.EnsureCreatedAsync();
                var ds = await db.ImportedDatasets.FirstOrDefaultAsync(d => d.Id == id);
                if (ds == null) return new DataTable();
                return DeserializeDataTable(ds.DataJson);
            }
        }

        private string SerializeDataTable(DataTable table)
        {
            if (table == null) return "";
            return JsonConvert.SerializeObject(table, Formatting.None);
        }

        private DataTable DeserializeDataTable(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new DataTable();
            return JsonConvert.DeserializeObject<DataTable>(json) ?? new DataTable();
        }
    }
}
