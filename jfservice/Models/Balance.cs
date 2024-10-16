namespace jfservice.Models
{
    public class Balance
    {
        public int account_id { get; set; }
        public int period { get; set; }
        public decimal in_balance { get; set; }//надеюсь, я правильно понял и это баланс клиента, а не поставщика услуг
        public decimal calculation { get; set; }
        public int year => period / 100;
        public int month => period % 100;
        public int quarter => (month + 2) / 3;
    }
}