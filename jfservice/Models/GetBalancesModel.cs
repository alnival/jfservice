using System.ComponentModel.DataAnnotations;

namespace jfservice.Models
{
    public class GetBalancesResponseModel
    {
        public string PeriodName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AmountAccrued { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ClosingBalance => OpeningBalance - AmountAccrued + AmountPaid;
    }

    public class GetBalancesRequestModel
    {
        [Required]
        public int accountId { get; set; } = 808251;
        //[RegularExpression("^(год|квартал|мес€ц)$", ErrorMessage = "PeriodType может принимать значени€ только 'год', 'квартал' или 'мес€ц'.")]
        //[Required]
        //public string periodType { get; set; }
        [EnumDataType(typeof(PeriodType), ErrorMessage = "PeriodType может принимать значени€ только '√од' - 0, ' вартал' - 1 или 'ћес€ц' - 2.")]
        public PeriodType periodType { get; set; }
    }

    public enum PeriodType
    {
        √од,
         вартал,
        ћес€ц,
    }

    public class GetBalancesStartingModel
    {
        public decimal Balance { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
    }
}