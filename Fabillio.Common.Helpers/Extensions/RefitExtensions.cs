using System.Collections.Generic;
using Refit;
using Fabillio.Common.Exceptions.Exceptions;
using Fabillio.Common.Helpers.Enums;

namespace Fabillio.Common.Helpers.Extensions;

public static class RefitExtensions
{
    public static T RetrieveContentFromResponse<T>(this ApiResponse<T> response, ClientType clientType = ClientType.Softrig) where T : class
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpClientErrorException($"In client: {clientType}, Message: {response.Error?.Message}, Content: {response.Error?.Content}");

        return response.Content;
    }

    public static IEnumerable<T> RetrieveContentFromResponse<T>(this ApiResponse<IEnumerable<T>> response, ClientType clientType = ClientType.Softrig) where T : class
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpClientErrorException($"In client: {clientType}, Message: {response.Error?.Message}, Content: {response.Error?.Content}");

        return response.Content;
    }
}
