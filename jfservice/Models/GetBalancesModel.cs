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
        //public decimal ClosingBalance => OpeningBalance - AmountAccrued + AmountPaid;//xml ���������� ������-�� �� ��������� ����������� � ������ ����
    }

    public class GetBalancesRequestModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "accountId ������ ���� ������������� ������.")]
        public int accountId { get; set; }
        //[RegularExpression("^(���|�������|�����)$", ErrorMessage = "periodType ����� ��������� �������� ������ '���', '�������' ��� '�����'.")]
        //public string periodType { get; set; }
        [Required(ErrorMessage = "periodType �� ������ ���� ������.")]
        [EnumDataType(typeof(PeriodType), ErrorMessage = "periodType ����� ��������� �������� ������ '���' - 0, '�������' - 1 ��� '�����' - 2.")]
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