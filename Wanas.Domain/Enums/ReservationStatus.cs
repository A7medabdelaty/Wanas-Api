namespace Wanas.Domain.Enums
{
    public enum ReservationStatus
    {
        PendingOwnerApproval = 0,
        AwaitingDepositPayment = 1,
        DepositPaid = 2,
        Confirmed = 3,
        Cancelled = 4
    }
}
