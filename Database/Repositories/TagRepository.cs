using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq.Expressions;
using Dapper;
using DataVisualizationApp.Database.Entities;

namespace DataVisualizationApp.Database.Repositories
{
    public class TagRepository : IRepository<Tag>
    {
        private readonly DatabaseHelper _dbHelper;

        public TagRepository()
        {
            _dbHelper = DatabaseHelper.Instance;
        }

        public Tag? GetById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Tag>(
                    "SELECT * FROM Tags WHERE Id = @Id",
                    new { Id = id });
            }
        }

        public IEnumerable<Tag> GetAll()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.Query<Tag>("SELECT * FROM Tags");
            }
        }

        public IEnumerable<Tag> Find(Expression<Func<Tag, bool>> predicate)
        {
            // Dapper不直接支持表达式树，这里简化实现
            throw new NotImplementedException("Find method not implemented for TagRepository");
        }

        public int Insert(Tag entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QuerySingle<int>(
                    @"INSERT INTO Tags (Name)
                      VALUES (@Name);
                      SELECT last_insert_rowid();",
                    entity);
            }
        }

        public void Update(Tag entity)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    @"UPDATE Tags
                      SET Name = @Name
                      WHERE Id = @Id",
                    entity);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute("DELETE FROM Tags WHERE Id = @Id", new { Id = id });
            }
        }

        // 额外的方法
        public Tag? GetTagByName(string name)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Tag>(
                    "SELECT * FROM Tags WHERE Name = @Name",
                    new { Name = name });
            }
        }

        public void AddTagToFile(int fileId, int tagId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    @"INSERT OR IGNORE INTO ExcelFileTags (ExcelFileId, TagId)
                      VALUES (@FileId, @TagId)",
                    new { FileId = fileId, TagId = tagId });
            }
        }

        public void RemoveTagFromFile(int fileId, int tagId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                connection.Execute(
                    @"DELETE FROM ExcelFileTags
                      WHERE ExcelFileId = @FileId AND TagId = @TagId",
                    new { FileId = fileId, TagId = tagId });
            }
        }

        public IEnumerable<Tag> GetTagsByFileId(int fileId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                return connection.Query<Tag>(
                    @"SELECT t.* FROM Tags t
                      INNER JOIN ExcelFileTags eft ON t.Id = eft.TagId
                      WHERE eft.ExcelFileId = @FileId",
                    new { FileId = fileId });
            }
        }
    }
}