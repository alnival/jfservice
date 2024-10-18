using Microsoft.AspNetCore.Mvc;

namespace jfservice.Formatters
{
    //не нашел стандартного обработчика для csv
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
            var csvData = "PeriodName;OpeningBalance;AmountAccrued;AmountPaid;ClosingBalance\n" +
                string.Join("\n", _data.Select(item => string.Join(";", item.GetType().GetProperties().Select(prop => prop.GetValue(item)))));
            return response.WriteAsync(csvData);
        }
    }
}