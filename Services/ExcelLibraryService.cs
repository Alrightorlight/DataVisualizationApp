using System;
using System.Collections.Generic;
using System.IO;
using DataVisualizationApp.Database.Entities;
using DataVisualizationApp.Database.Repositories;
using OfficeOpenXml;

namespace DataVisualizationApp.Services
{
    public class ExcelLibraryService
    {
        private readonly ExcelFileRepository _excelFileRepository;
        private readonly ExcelSheetRepository _excelSheetRepository;
        private readonly TagRepository _tagRepository;
        private readonly ExcelService _excelService;

        public ExcelLibraryService()
        {
            _excelFileRepository = new ExcelFileRepository();
            _excelSheetRepository = new ExcelSheetRepository();
            _tagRepository = new TagRepository();
            _excelService = new ExcelService();
        }

        // 导入Excel文件到库中
        public int ImportExcelFile(string filePath, string category = "General", string? description = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            try
            {
                // 获取文件信息
                var fileInfo = new FileInfo(filePath);
                var excelFileInfo = _excelService.GetExcelInfo(filePath);

                // 创建ExcelFile实体
                var excelFile = new ExcelFile
                {
                    FilePath = filePath,
                    FileName = fileInfo.Name,
                    OriginalFileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    ImportDate = DateTime.Now,
                    LastModified = fileInfo.LastWriteTime,
                    Category = category,
                    Description = description
                };

                // 保存到数据库
                int fileId = _excelFileRepository.Insert(excelFile);

                // 保存工作表信息
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    foreach (var sheetName in excelFileInfo.WorksheetNames)
                    {
                        var worksheet = package.Workbook.Worksheets[sheetName];
                        var sheet = new ExcelSheet
                        {
                            ExcelFileId = fileId,
                            SheetName = sheetName,
                            RowCount = worksheet.Dimension?.Rows ?? 0,
                            ColumnCount = worksheet.Dimension?.Columns ?? 0
                        };
                        _excelSheetRepository.Insert(sheet);
                    }
                }

                return fileId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error importing Excel file", ex);
            }
        }

        // 获取所有Excel文件
        public IEnumerable<ExcelFile> GetAllExcelFiles()
        {
            return _excelFileRepository.GetAll();
        }

        // 根据ID获取Excel文件
        public ExcelFile? GetExcelFileById(int id)
        {
            var file = _excelFileRepository.GetById(id);
            if (file != null)
            {
                // 更新最后访问时间
                _excelFileRepository.UpdateLastAccessed(id);
                // 获取关联的工作表
                file.Sheets = _excelSheetRepository.GetSheetsByFileId(id).ToList();
                // 获取关联的标签
                file.Tags = _tagRepository.GetTagsByFileId(id).ToList();
            }
            return file;
        }

        // 更新Excel文件信息
        public void UpdateExcelFile(ExcelFile excelFile)
        {
            excelFile.LastModified = DateTime.Now;
            _excelFileRepository.Update(excelFile);
        }

        // 删除Excel文件
        public void DeleteExcelFile(int id)
        {
            // 先删除关联的标签关系
            var tags = _tagRepository.GetTagsByFileId(id);
            foreach (var tag in tags)
            {
                _tagRepository.RemoveTagFromFile(id, tag.Id);
            }

            // 再删除工作表
            var sheets = _excelSheetRepository.GetSheetsByFileId(id);
            foreach (var sheet in sheets)
            {
                _excelSheetRepository.Delete(sheet.Id);
            }

            // 最后删除文件记录
            _excelFileRepository.Delete(id);
        }

        // 添加标签
        public int AddTag(string tagName)
        {
            var existingTag = _tagRepository.GetTagByName(tagName);
            if (existingTag != null)
                return existingTag.Id;

            var tag = new Tag { Name = tagName };
            return _tagRepository.Insert(tag);
        }

        // 为文件添加标签
        public void AddTagToFile(int fileId, string tagName)
        {
            int tagId = AddTag(tagName);
            _tagRepository.AddTagToFile(fileId, tagId);
        }

        // 从文件移除标签
        public void RemoveTagFromFile(int fileId, string tagName)
        {
            var tag = _tagRepository.GetTagByName(tagName);
            if (tag != null)
            {
                _tagRepository.RemoveTagFromFile(fileId, tag.Id);
            }
        }

        // 获取所有标签
        public IEnumerable<Tag> GetAllTags()
        {
            return _tagRepository.GetAll();
        }

        // 根据标签获取文件
        public IEnumerable<ExcelFile> GetFilesByTag(string tagName)
        {
            var tag = _tagRepository.GetTagByName(tagName);
            if (tag == null)
                return new List<ExcelFile>();

            // 这个查询比较复杂，需要通过ExcelFileTags连接表
            // 实际应用中可能需要在仓储层添加专门的方法
            throw new NotImplementedException("GetFilesByTag method not implemented");
        }

        // 读取Excel文件数据
        public System.Data.DataTable ReadExcelFileData(int fileId, string sheetName)
        {
            var filePath = _excelFileRepository.GetFilePathById(fileId);
            if (string.IsNullOrEmpty(filePath))
                throw new Exception("File not found in database");

            return _excelService.ReadExcelData(filePath, sheetName);
        }
    }
}