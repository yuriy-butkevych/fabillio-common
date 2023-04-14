using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using Fabillio.Common.Clients.Oslofjord.Models.Requests;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Verifone;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Cards;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Contracts;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Persons;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Verifone;

namespace Fabillio.Common.Clients.Oslofjord.Interfaces;

public interface IOslofjordSamvirkService
{
    Task<CardDetailsResponse> GetCardByPersonId(
        string bccPersonId,
        CancellationToken cancellationToken
    );
    Task<IEnumerable<CardResponse>> GetCards(
        CancellationToken cancellationToken,
        DateTime? modifiedAfter = null
    );
    Task<IEnumerable<TransactionResponse>> GetTransactions(
        CancellationToken cancellationToken,
        [Query] TransactionQueryRequest transactionQuery = null
    );
    Task Refund(RefundRequest refundRequest, CancellationToken cancellationToken);
    Task RecalculateReservationPrice(string reservationNumber, CancellationToken cancellationToken);
    Task<PersonResponse> GetPersonByKey(string personKey, CancellationToken cancellationToken);
    Task<IEnumerable<ContractResponse>> GetContractsByPersonKey(
        string personKey,
        CancellationToken cancellationToken
    );
    Task<InitializeVerifonePaymentResponse> InitializeVerifonePayment(
        InitializeVerifonePaymentRequest request,
        CancellationToken cancellationToken
    );
}
