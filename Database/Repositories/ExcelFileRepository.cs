using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq.Expressions;
using Dapper;
using DataVisualizationApp.Database.Entities;

namespace DataVisualizationApp.Database.Repositories
{
    public class ExcelFileRepository : IRepository<ExcelFile>
    {
        private readonly DatabaseHelper _dbHelper;

        public ExcelFileRepository()
        {
            _dbHelper = DatabaseHelper.Instance;
        }

        public ExcelFile? GetById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QueryFirstOrDefault<ExcelFile>(
                    "SELECT * FROM ExcelFiles WHERE Id = @Id",
                    new { Id = id });
            }
        }

        public IEnumerable<ExcelFile> GetAll()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.Query<ExcelFile>("SELECT * FROM ExcelFiles");
            }
        }

        public IEnumerable<ExcelFile> Find(Expression<Func<ExcelFile, bool>> predicate)
        {
            // Dapper不直接支持表达式树，这里简化实现
            // 实际应用中可以使用PredicatesBuilder或其他库转换表达式树为SQL
            throw new NotImplementedException("Find method not implemented for ExcelFileRepository");
        }

        public int Insert(ExcelFile entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QuerySingle<int>(
                    @"INSERT INTO ExcelFiles (FilePath, FileName, OriginalFileName, FileSize, ImportDate, LastModified, Category, Description)
                      VALUES (@FilePath, @FileName, @OriginalFileName, @FileSize, @ImportDate, @LastModified, @Category, @Description);
                      SELECT last_insert_rowid();",
                    entity);
            }
        }

        public void Update(ExcelFile entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    @"UPDATE ExcelFiles
                      SET FilePath = @FilePath,
                          FileName = @FileName,
                          OriginalFileName = @OriginalFileName,
                          FileSize = @FileSize,
                          ImportDate = @ImportDate,
                          LastModified = @LastModified,
                          Category = @Category,
                          Description = @Description,
                          LastAccessed = @LastAccessed
                      WHERE Id = @Id",
                    entity);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute("DELETE FROM ExcelFiles WHERE Id = @Id", new { Id = id });
            }
        }

        // 额外的方法
        public string GetFilePathById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                var filePath = connection.QueryFirstOrDefault<string>(
                    "SELECT FilePath FROM ExcelFiles WHERE Id = @Id",
                    new { Id = id });

                if (filePath == null)
                {
                    throw new KeyNotFoundException($"Excel文件不存在，ID: {id}");
                }
                return filePath;
            }
        }

        public void UpdateLastAccessed(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    "UPDATE ExcelFiles SET LastAccessed = @LastAccessed WHERE Id = @Id",
                    new { LastAccessed = DateTime.Now, Id = id });
            }
        }
    }
}