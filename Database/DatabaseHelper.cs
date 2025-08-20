using System;
using System.Data.SQLite;
using System.IO;

namespace DataVisualizationApp.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private static readonly object _lock = new object();
        private static DatabaseHelper? _instance;

        private DatabaseHelper()
        {
            // 数据库文件路径
            string dbPath = Path.Combine(Environment.CurrentDirectory, "ExcelLibrary.db");
            _connectionString = $"Data Source={dbPath};Version=3;";

            // 初始化数据库
            InitializeDatabase();
        }

        public static DatabaseHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        private void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // 创建ExcelFiles表
                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS ExcelFiles (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FilePath TEXT NOT NULL UNIQUE,
                        FileName TEXT NOT NULL,
                        OriginalFileName TEXT NOT NULL,
                        FileSize INTEGER NOT NULL,
                        ImportDate TIMESTAMP NOT NULL,
                        LastModified TIMESTAMP NOT NULL,
                        Category TEXT DEFAULT 'General',
                        Description TEXT,
                        LastAccessed TIMESTAMP
                    )
                ");

                // 创建ExcelSheets表
                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS ExcelSheets (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ExcelFileId INTEGER NOT NULL,
                        SheetName TEXT NOT NULL,
                        RowCount INTEGER NOT NULL,
                        ColumnCount INTEGER NOT NULL,
                        FOREIGN KEY (ExcelFileId) REFERENCES ExcelFiles(Id) ON DELETE CASCADE
                    )
                ");

                // 创建Tags表
                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS Tags (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE
                    )
                ");

                // 创建ExcelFileTags表
                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS ExcelFileTags (
                        ExcelFileId INTEGER NOT NULL,
                        TagId INTEGER NOT NULL,
                        PRIMARY KEY (ExcelFileId, TagId),
                        FOREIGN KEY (ExcelFileId) REFERENCES ExcelFiles(Id) ON DELETE CASCADE,
                        FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                    )
                ");
            }
        }

        public void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                return command.ExecuteScalar();
            }
        }
    }
}