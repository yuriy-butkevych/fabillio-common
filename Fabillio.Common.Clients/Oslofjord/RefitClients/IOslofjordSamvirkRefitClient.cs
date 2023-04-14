using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using Fabillio.Common.Clients.Oslofjord.Constants;
using Fabillio.Common.Clients.Oslofjord.Models.Requests;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Verifone;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Cards;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Contracts;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Persons;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Verifone;

namespace Fabillio.Common.Clients.Oslofjord.RefitClients;

public interface IOslofjordSamvirkRefitClient
{
    [Get(OslofjordClientConstants.HttpRoutes._cards)]
    Task<ApiResponse<IEnumerable<CardResponse>>> GetCards(
        [Query] DateTime? modifiedAfter,
        CancellationToken cancellationToken
    );

    [Get(OslofjordClientConstants.HttpRoutes._cardsByPerson)]
    Task<ApiResponse<CardDetailsResponse>> GetCardById(
        string personKey,
        CancellationToken cancellationToken
    );

    [Get(OslofjordClientConstants.HttpRoutes._transactions)]
    Task<ApiResponse<IEnumerable<TransactionResponse>>> GetTransactions(
        CancellationToken cancellationToken,
        [Query] TransactionQueryRequest transactionQuery = null
    );

    [Post(OslofjordClientConstants.HttpRoutes._refund)]
    Task Refund([Query] RefundRequest request, CancellationToken cancellationToken);

    [Get(OslofjordClientConstants.HttpRoutes._reservationRecalculatePrice)]
    Task RecalculateReservationPrice(
        [Query] string reservationNumber,
        CancellationToken cancellationToken
    );

    [Get(OslofjordClientConstants.HttpRoutes._personByKey)]
    Task<ApiResponse<PersonResponse>> GetPersonByKey(
        string personKey,
        CancellationToken cancellationToken
    );

    [Get(OslofjordClientConstants.HttpRoutes._contractsByPersonKey)]
    Task<ApiResponse<IEnumerable<ContractResponse>>> GetContractsByPersonKey(
        string personKey,
        CancellationToken cancellationToken
    );

    [Post(OslofjordClientConstants.HttpRoutes._verifoneInitializePayment)]
    Task<ApiResponse<InitializeVerifonePaymentResponse>> InitializeVerifonePayment(
        [Body] InitializeVerifonePaymentRequest request,
        CancellationToken cancellationToken
    );
}
