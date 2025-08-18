using System;
using System.IO;
using DataVisualizationApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DataVisualizationApp.DataBase
{
    public class AppDbContext : DbContext
    {
        public DbSet<ImportedDataset> ImportedDatasets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "app.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImportedDataset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.DataJson).IsRequired();
            });
        }
    }
}
