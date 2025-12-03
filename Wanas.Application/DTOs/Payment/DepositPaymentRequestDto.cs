namespace Wanas.Application.DTOs.Payment
{
    public class DepositPaymentRequestDto
    {
        public string PaymentToken { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
