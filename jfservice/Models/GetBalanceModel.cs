namespace jfservice.Models
{
    public class GetBalanceModel
    {
        public string PeriodName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AmountAccrued { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ClosingBalance => OpeningBalance - AmountAccrued + AmountPaid;
    }
}