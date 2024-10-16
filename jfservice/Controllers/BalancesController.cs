using jfservice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace jfservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BalancesController : ControllerBase
    {
        private readonly ILogger<BalancesController> _logger;
        private readonly List<Balance> Balances = new();
        private readonly List<Payment> Payments = new();

        public BalancesController(ILogger<BalancesController> logger)
        {
            _logger = logger;
            Balances = LoadDataFromFile<Balance>("balance_202105270825.json");
            Payments = LoadDataFromFile<Payment>("payment_202105270827.json");
        }

        private List<T> LoadDataFromFile<T>(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName);
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<T>>(json);
            }
            else
            {
                _logger.LogError($"���� {fileName} �� ������.");
                return new List<T>();
            }
        }

        private BadRequestObjectResult? Validation(int accountId, string periodType)
        {
            if (accountId < 0)
            {
                return BadRequest(new { message = "accountId �� ����� ���� �������������." });
            }
            if (!(new List<string>() { "���", "��������", "�����" }.Contains(periodType.ToLower())))
            {
                return BadRequest(new { message = "periodType ����� ��������� ������ ��������� ��������: '���', '�������', '�����'." });
            }
            //�� ���� ��� ��� ����� ���������
            return null;
        }

        [HttpGet("{accountId}")]
        public IActionResult GetBalances(int accountId = 808251, string periodType = "���")
        {
            var validationResult = Validation(accountId, periodType);
            if (validationResult != null) { return validationResult; }
            var balances = Balances
                .Where(_ => _.account_id == accountId)
                .OrderBy(_ => _.period);
            var payments = Payments
                .Where(_ => _.account_id == accountId);
            if (!balances.Any()) { return BadRequest(new { message = "���� �������� �� ������ ��� � ��� ��� �������." }); }
            if (!payments.Any()) { return BadRequest(new { message = "���� �������� �� ������ ��� � ��� ��� �������." }); }
            var startingBalance = balances.FirstOrDefault().in_balance;
            var startingYear = balances.FirstOrDefault().year;
            var startingMonth = balances.FirstOrDefault().month;
            var startingQuarter = balances.FirstOrDefault().quarter;
            var result = new List<GetBalanceModel>();
            switch (periodType.ToLower())
            {
                case "���":
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        result.Add(new GetBalanceModel
                        {
                            PeriodName = year.ToString(),
                            OpeningBalance = year == startingYear ? startingBalance : result.Last().ClosingBalance,
                            AmountAccrued = balances.Where(_ => _.year == year).Sum(_ => _.calculation),
                            AmountPaid = payments.Where(_ => _.year == year).Sum(_ => _.sum),
                        });
                    }
                    break;
                case "�������":
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        foreach (var quarter in balances.Where(_ => _.year == year).Select(_ => _.quarter).Concat(payments.Where(_ => _.year == year).Select(_ => _.quarter)).Distinct().Order())
                        {
                            result.Add(new GetBalanceModel
                            {
                                PeriodName = $"{year} ��� {quarter} ��������",
                                OpeningBalance = year == startingYear & quarter == startingQuarter ? startingBalance : result.Last().ClosingBalance,
                                AmountAccrued = balances.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.calculation),
                                AmountPaid = payments.Where(_ => _.year == year & _.quarter == quarter).Sum(_ => _.sum),
                            });
                        }
                    }
                    break;
                case "�����":
                    foreach (var year in balances.Select(_ => _.year).Concat(payments.Select(_ => _.year)).Distinct().Order())
                    {
                        foreach (var month in balances.Where(_ => _.year == year).Select(_ => _.month).Concat(payments.Where(_ => _.year == year).Select(_ => _.month)).Distinct().Order())
                        {
                            result.Add(new GetBalanceModel
                            {
                                PeriodName = $"{year} ��� {month:00} ����� ({CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU").DateTimeFormat.GetMonthName(month)})",
                                OpeningBalance = year == startingYear & month == startingMonth ? startingBalance : result.Last().ClosingBalance,
                                AmountAccrued = balances.Where(_ => _.year == year & _.month == month).Sum(_ => _.calculation),
                                AmountPaid = payments.Where(_ => _.year == year & _.month == month).Sum(_ => _.sum),
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
            return FormatResponse(result.OrderByDescending(_ => _.PeriodName).ToList());
        }

        private IActionResult FormatResponse(List<GetBalanceModel> model)
        {
            if (Request.Headers.Accept.ToString().Contains("application/xml"))
            {
                var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<GetBalanceModel>));
                using var stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, model);
                return Content(stringWriter.ToString(), "application/xml");
            }
            else if (Request.Headers.Accept.ToString().Contains("text/csv"))
            {
                var csv = "PeriodName,OpeningBalance,AmountAccrued,AmountPaid,ClosingBalance\n" +
                    string.Join("\n", model.Select(_ => $"{_.PeriodName},{_.OpeningBalance},{_.AmountAccrued},{_.AmountPaid},{_.ClosingBalance}"));
                return Content(csv, "text/csv");
            }
            else
            {
                return Ok(model);
            }
        }

        [HttpGet("{accountId}/debt")]
        public IActionResult GetDebt(int accountId = 808251)
        {
            var totalAccrued = Balances
                .Where(_ => _.account_id == accountId)
                .Sum(_ => _.calculation);
            var totalPayments = Payments
                .Where(_ => _.account_id == accountId)
                .Sum(_ => _.sum);
            decimal debt = totalAccrued - totalPayments;
            return Ok(new { AccountId = accountId, Debt = debt });
        }
    }
}