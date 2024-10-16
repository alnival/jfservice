namespace jfservice.Models
{
    public class Payment
    {
        public int account_id { get; set; }
        public string date { get; set; }
        public decimal sum { get; set; }
        public string payment_guid { get; set; }
        public DateTime date_time => DateTime.Parse(date);
        public int year => date_time.Year;
        public int month => date_time.Month;
        public int quarter=> (month + 2) / 3;
    }
}