using jfservice.Interfaces;
using System.Text.Json;

namespace jfservice.Services
{
    public class DataLoaderService : IDataLoaderService
    {
        private readonly ILogger<DataLoaderService> _logger;

        public DataLoaderService(ILogger<DataLoaderService> logger)
        {
            _logger = logger;
        }

        public List<T> LoadData<T>(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName);
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                try
                {
                    var result = JsonSerializer.Deserialize<List<T>>(json);
                    if (result == null)
                    {
                        _logger.LogError($"Невозможно получить даныне из файла {fileName}.");
                        throw new DataFileBadFormatException(fileName);
                    }
                    return result;
                }
                catch (Exception)
                {
                    _logger.LogError($"Невозможно получить даныне из файла {fileName}.");
                    throw new DataFileBadFormatException(fileName);
                }
            }
            else
            {
                _logger.LogError($"Файл {fileName} не найден.");
                throw new DataFileNotFoundException(fileName);
            }
        }

        public class DataFileBadFormatException : Exception
        {
            public DataFileBadFormatException(string fileName) : base($"Невозможно получить даныне из файла {fileName}.") { }
        }

        public class DataFileNotFoundException : Exception
        {
            public DataFileNotFoundException(string fileName) : base($"Файл {fileName} не найден.") { }
        }
    }
}