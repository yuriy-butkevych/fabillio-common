namespace Fabillio.Common.Clients.Oslofjord.Constants;

public static class OslofjordClientConstants
{
    public static class HttpRoutes
    {
        public const string _cards = "/cards";
        public const string _cardsByPerson = "/cards/{personKey}";

        public const string _transactions = "/transactions";
        public const string _refund = "/refund";
        public const string _reservationRecalculatePrice =
            "/samvirk-reservation/reservation-updated/{reservationNumber}";

        public const string _personByKey = "/persons/person/{personKey}";

        public const string _contractsByPersonKey = "/contracts/GetByPersonKey/{personKey}";

        public const string _verifoneInitializePayment = "/verifone/initialize-payment";
    }
}
