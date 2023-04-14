using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Refit;
using Fabillio.Common.Clients.Oslofjord.Models.Requests;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Requests.Verifone;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Cards;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Contracts;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Persons;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Transactions;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Verifone;
using Fabillio.Common.Helpers.Enums;
using Fabillio.Common.Helpers.Extensions;

namespace Fabillio.Common.Clients.Oslofjord.Interfaces.Implementations;

public class OslofjordSamvirkClient : IOslofjordSamvirkService
{
    public const string HttpClientName = "OslofjordSamvirkApiHttpClient";
    private readonly RefitClients.IOslofjordSamvirkRefitClient _client;
    private readonly ILogger<OslofjordSamvirkClient> _logger;

    public OslofjordSamvirkClient(
        IHttpClientFactory httpClientFactory,
        ILogger<OslofjordSamvirkClient> logger
    )
    {
        var httpClient = EnsureArg
            .IsNotNull(httpClientFactory, nameof(httpClientFactory))
            .CreateClient(HttpClientName);
        _logger = logger;

        _client = RestService.For<RefitClients.IOslofjordSamvirkRefitClient>(httpClient);
    }

    public async Task<CardDetailsResponse> GetCardByPersonId(
        string bccPersonId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _client.GetCardById(bccPersonId, cancellationToken);

            return response.RetrieveContentFromResponse(ClientType.Oslofjord);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task<IEnumerable<CardResponse>> GetCards(
        CancellationToken cancellationToken,
        DateTime? modifiedAfter = null
    )
    {
        try
        {
            var response = await _client.GetCards(modifiedAfter, cancellationToken);

            return response.RetrieveContentFromResponse(ClientType.Oslofjord);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task<IEnumerable<TransactionResponse>> GetTransactions(
        CancellationToken cancellationToken,
        [Query] TransactionQueryRequest transactionQuery = null
    )
    {
        try
        {
            var response = await _client.GetTransactions(cancellationToken, transactionQuery);

            return response.RetrieveContentFromResponse(ClientType.Oslofjord);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task Refund(RefundRequest refundRequest, CancellationToken cancellationToken)
    {
        try
        {
            await _client.Refund(refundRequest, cancellationToken);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task RecalculateReservationPrice(
        string reservationNumber,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _client.RecalculateReservationPrice(reservationNumber, cancellationToken);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task<PersonResponse> GetPersonByKey(
        string personKey,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _client.GetPersonByKey(personKey, cancellationToken);

            if (response?.StatusCode is not HttpStatusCode.OK)
                return null;

            response.Content?.TrimStringProperties();
            return response.RetrieveContentFromResponse();
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task<IEnumerable<ContractResponse>> GetContractsByPersonKey(
        string personKey,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _client.GetContractsByPersonKey(personKey, cancellationToken);

            if (response?.StatusCode is not HttpStatusCode.OK)
                return null;

            return response.RetrieveContentFromResponse(ClientType.Oslofjord);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }

    public async Task<InitializeVerifonePaymentResponse> InitializeVerifonePayment(
        InitializeVerifonePaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _client.InitializeVerifonePayment(request, cancellationToken);

            if (response?.StatusCode is not HttpStatusCode.OK)
                return null;

            return response.RetrieveContentFromResponse(ClientType.Oslofjord);
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                cancellationException,
                $"{nameof(OslofjordSamvirkClient)} task cancelled {cancellationException}"
            );
            throw cancellationException;
        }
    }
}
