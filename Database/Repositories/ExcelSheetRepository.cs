using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq.Expressions;
using Dapper;
using DataVisualizationApp.Database.Entities;

namespace DataVisualizationApp.Database.Repositories
{
    public class ExcelSheetRepository : IRepository<ExcelSheet>
    {
        private readonly DatabaseHelper _dbHelper;

        public ExcelSheetRepository()
        {
            _dbHelper = DatabaseHelper.Instance;
        }

        public ExcelSheet? GetById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QueryFirstOrDefault<ExcelSheet>(
                    "SELECT * FROM ExcelSheets WHERE Id = @Id",
                    new { Id = id });
            }
        }

        public IEnumerable<ExcelSheet> GetAll()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.Query<ExcelSheet>("SELECT * FROM ExcelSheets");
            }
        }

        public IEnumerable<ExcelSheet> Find(Expression<Func<ExcelSheet, bool>> predicate)
        {
            // Dapper不直接支持表达式树，这里简化实现
            throw new NotImplementedException("Find method not implemented for ExcelSheetRepository");
        }

        public int Insert(ExcelSheet entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QuerySingle<int>(
                    @"INSERT INTO ExcelSheets (ExcelFileId, SheetName, RowCount, ColumnCount)
                      VALUES (@ExcelFileId, @SheetName, @RowCount, @ColumnCount);
                      SELECT last_insert_rowid();",
                    entity);
            }
        }

        public void Update(ExcelSheet entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    @"UPDATE ExcelSheets
                      SET ExcelFileId = @ExcelFileId,
                          SheetName = @SheetName,
                          RowCount = @RowCount,
                          ColumnCount = @ColumnCount
                      WHERE Id = @Id",
                    entity);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute("DELETE FROM ExcelSheets WHERE Id = @Id", new { Id = id });
            }
        }

        // 额外的方法
        public IEnumerable<ExcelSheet> GetSheetsByFileId(int fileId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.Query<ExcelSheet>(
                    "SELECT * FROM ExcelSheets WHERE ExcelFileId = @FileId",
                    new { FileId = fileId });
            }
        }
    }
}