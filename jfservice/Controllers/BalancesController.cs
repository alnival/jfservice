using jfservice.Formatters;
using jfservice.Interfaces;
using jfservice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace jfservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BalancesController : ControllerBase
    {
        private readonly ILogger<BalancesController> _logger;
        private readonly IDataLoaderService _dataLoaderService;
        private readonly FileSettings _fileSettings;
        private readonly List<Balance> _balances;
        private readonly List<Payment> _payments;

        public BalancesController(ILogger<BalancesController> logger, IDataLoaderService dataLoaderService, IOptions<FileSettings> fileSettings)
        {
            _logger = logger;
            _dataLoaderService = dataLoaderService;
            _fileSettings = fileSettings.Value;
            try
            {
                _balances = _dataLoaderService.LoadData<Balance>(_fileSettings.BalanceFile);
                _payments = _dataLoaderService.LoadData<Payment>(_fileSettings.PaymentFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при загрузке данных.");
                NoContent();
            }
        }

        [HttpGet("{accountId}/{periodType}")]
        public IActionResult GetBalances([FromRoute] GetBalancesRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var balances = _balances
                .Where(_ => _.account_id == request.accountId)
                .OrderBy(_ => _.period);
            var payments = _payments
                .Where(_ => _.account_id == request.accountId);
            if (!balances.Any()) { return BadRequest(new { ErrorMessage = "В файле балансов нет записей для этого аккаунта." }); }
            //if (!payments.Any()) { return BadRequest(new { ErrorMessage = "В файле платежей нет записей для этого аккаунта." }); }//в принципе оплат могло и не быть
            var starting = new GetBalancesStartingModel()
            {
                Balance = balances.FirstOrDefault().in_balance,
                Year = balances.FirstOrDefault().year,
                Month = balances.FirstOrDefault().month,
                Quarter = balances.FirstOrDefault().quarter,
            };
            var response = new List<GetBalancesResponseModel>();
            switch (request.periodType)
            {
                case PeriodType.Год:
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        response.Add(new GetBalancesResponseModel
                        {
                            PeriodName = year.ToString(),
                            OpeningBalance = year == starting.Year ? starting.Balance : response.Last().ClosingBalance,
                            AmountAccrued = balances.Where(_ => _.year == year).Sum(_ => _.calculation),
                            AmountPaid = payments.Where(_ => _.year == year).Sum(_ => _.sum),
                            //xml не возращает вычисляемое в модели поле, поэтому придется считать здесь
                            ClosingBalance = (year == starting.Year ? starting.Balance : response.Last().ClosingBalance) - (balances.Where(_ => _.year == year).Sum(_ => _.calculation)) + (payments.Where(_ => _.year == year).Sum(_ => _.sum)),
                        });
                    }
                    break;
                case PeriodType.Квартал:
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        foreach (var quarter in balances.Where(_ => _.year == year).Select(_ => _.quarter).Concat(payments.Where(_ => _.year == year).Select(_ => _.quarter)).Distinct().Order())
                        {
                            response.Add(new GetBalancesResponseModel
                            {
                                PeriodName = $"{year} год {quarter} кваратал",
                                OpeningBalance = year == starting.Year & quarter == starting.Quarter ? starting.Balance : response.Last().ClosingBalance,
                                AmountAccrued = balances.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.calculation),
                                AmountPaid = payments.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.sum),
                                //xml не возращает вычисляемое в модели поле, поэтому придется считать здесь
                                ClosingBalance = (year == starting.Year & quarter == starting.Quarter ? starting.Balance : response.Last().ClosingBalance) - (balances.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.calculation)) + (payments.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.sum)),
                            });
                        }
                    }
                    break;
                case PeriodType.Месяц:
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        foreach (var month in balances.Where(_ => _.year == year).Select(_ => _.month).Concat(payments.Where(_ => _.year == year).Select(_ => _.month)).Distinct().Order())
                        {
                            response.Add(new GetBalancesResponseModel
                            {
                                PeriodName = $"{year} год {month:00} месяц ({CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU").DateTimeFormat.GetMonthName(month)})",
                                OpeningBalance = year == starting.Year & month == starting.Month ? starting.Balance : response.Last().ClosingBalance,
                                AmountAccrued = balances.Where(_ => _.year == year & _.month == month).Sum(_ => _.calculation),
                                AmountPaid = payments.Where(_ => _.year == year & _.month == month).Sum(_ => _.sum),
                                //xml не возращает вычисляемое в модели поле, поэтому придется считать здесь
                                ClosingBalance = (year == starting.Year & month == starting.Month ? starting.Balance : response.Last().ClosingBalance) - (balances.Where(_ => _.year == year & _.month == month).Sum(_ => _.calculation)) + (payments.Where(_ => _.year == year & _.month == month).Sum(_ => _.sum)),
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
            //return FormatResponse(response.OrderByDescending(_ => _.PeriodName).ToList());//попробую воспользоваться стандартными обработчиками
            if (Request.Headers.Accept.ToString().Contains("text/csv"))
            {
                return new CsvResult(response.OrderByDescending(_ => _.PeriodName).ToList());
            }
            return Ok(response.OrderByDescending(_ => _.PeriodName).ToList());
        }

        //private IActionResult FormatResponse(List<GetBalancesResponseModel> model)
        //{
        //    if (Request.Headers.Accept.ToString().Contains("application/xml"))
        //    {
        //        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<GetBalancesResponseModel>));
        //        using var stringWriter = new StringWriter();
        //        xmlSerializer.Serialize(stringWriter, model);
        //        return Content(stringWriter.ToString(), "application/xml");
        //    }
        //    else if (Request.Headers.Accept.ToString().Contains("text/csv"))
        //    {
        //        var csv = "PeriodName;OpeningBalance;AmountAccrued;AmountPaid;ClosingBalance\n" +
        //            string.Join("\n", model.Select(_ => $"{_.PeriodName};{_.OpeningBalance};{_.AmountAccrued};{_.AmountPaid};{_.ClosingBalance}"));
        //        return Content(csv, "text/csv");
        //    }
        //    else
        //    {
        //        return Ok(model);
        //    }
        //}

        [HttpGet("{accountId}/debt")]
        public IActionResult GetDebt(int accountId = 808251)
        {
            var totalAccrued = _balances
                .Where(_ => _.account_id == accountId)
                .Sum(_ => _.calculation);
            var totalPayments = _payments
                .Where(_ => _.account_id == accountId)
                .Sum(_ => _.sum);
            decimal debt = totalAccrued - totalPayments;
            return Ok(new { AccountId = accountId, Debt = debt });
        }
    }
}