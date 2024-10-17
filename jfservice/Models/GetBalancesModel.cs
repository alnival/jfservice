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
        //[RegularExpression("^(���|�������|�����)$", ErrorMessage = "PeriodType ����� ��������� �������� ������ '���', '�������' ��� '�����'.")]
        //[Required]
        //public string periodType { get; set; }
        [EnumDataType(typeof(PeriodType), ErrorMessage = "PeriodType ����� ��������� �������� ������ '���' - 0, '�������' - 1 ��� '�����' - 2.")]
        public PeriodType periodType { get; set; }
    }

    public enum PeriodType
    {
        ���,
        �������,
        �����,
    }

    public class GetBalancesStartingModel
    {
        public decimal Balance { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
    }
}