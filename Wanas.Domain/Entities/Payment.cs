using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // reference id from payment gateway (Stripe, PayPal, Vodafone Cash, etc..)
        public string TransactionId { get; set; }
        public string Notes { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }
}
