using Microsoft.AspNetCore.Mvc;

namespace jfservice.Models
{
    public class CsvResult : IActionResult
    {
        private readonly IEnumerable<object> _data;

        public CsvResult(IEnumerable<object> data)
        {
            _data = data;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response; response.ContentType = "text/csv";
            var csvData = string.Join("\n", _data.Select(item => string.Join(";", item.GetType().GetProperties().Select(prop => prop.GetValue(item)))));
            return response.WriteAsync(csvData);
        }
    }
}