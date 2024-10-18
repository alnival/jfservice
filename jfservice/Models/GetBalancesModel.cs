using System.ComponentModel.DataAnnotations;

namespace jfservice.Models
{
    public class GetBalancesResponseModel
    {
        public string PeriodName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AmountAccrued { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ClosingBalance { get; set; }
        //public decimal ClosingBalance => OpeningBalance - AmountAccrued + AmountPaid;//xml обработчик почему-то не возращает вычисл¤емое в модели поле
    }

    public class GetBalancesRequestModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "accountId должен быть положительным числом.")]
        public int accountId { get; set; }
        //[RegularExpression("^(год|квартал|мес¤ц)$", ErrorMessage = "periodType может принимать значени¤ только 'год', 'квартал' или 'мес¤ц'.")]
        //public string periodType { get; set; }
        [Required(ErrorMessage = "periodType не должно быть пустым.")]
        [EnumDataType(typeof(PeriodType), ErrorMessage = "periodType может принимать значени¤ только '√од' - 0, ' вартал' - 1 или 'ћес¤ц' - 2.")]
        public PeriodType periodType { get; set; }
    }

    public enum PeriodType
    {
        √од,
         вартал,
        ћес¤ц,
    }

    public class GetBalancesStartingModel
    {
        public decimal Balance { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
    }
}