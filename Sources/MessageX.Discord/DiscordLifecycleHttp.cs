using System.IO;
using System.Net.Http;

namespace MessageX.Discord;

internal static class DiscordLifecycleHttp {
    public static async Task<TResult> ExecuteAsync<TResult>(
        HttpClient httpClient,
        HttpRequestMessage request,
        string operation,
        Func<HttpResponseMessage, string, TResult> createResult,
        CancellationToken cancellationToken) {
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            httpClient,
            cancellationToken);
        try {
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            var responseBody = await MessageHttpResponseReader
                .ReadUtf8BodyAsync(response, operationCancellation.Token)
                .ConfigureAwait(false);
            return createResult(response, responseBody);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException($"Discord {operation} request timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException($"Discord {operation} request failed.", MessageErrorKind.Transient);
        }
    }
}
