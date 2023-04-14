using System.Collections.Generic;

namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Verifone
{
    public class InitializeVerifonePaymentResponse
    {
        public Dictionary<string, string> Form { get; set; }

        public string VerifonePaymentUrl { get; set; }
    }
}
