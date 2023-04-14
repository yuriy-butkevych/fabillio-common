using System;

namespace Fabillio.Common.Clients.Oslofjord.Models.Requests.Verifone
{
    public class InitializeVerifonePaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentReference { get; set; }
        public Guid ContactId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CancelUrl { get; set; }
        public string SuccessUrl { get; set; }
    }
}
