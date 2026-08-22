using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MessageX.Slack;

internal sealed class SlackWebApiInvoker {
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;

    public SlackWebApiInvoker(SlackConnection connection, HttpClient httpClient) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<SlackDeliveryResult> ExecuteAsync(
        string method,
        string json,
        string safeTarget,
        Func<SlackApiResponse, bool> validateSuccess,
        Func<SlackApiResponse, string?, MessageReference?> createReference,
        CancellationToken cancellationToken) {
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(_connection.ApiBaseUri, method)) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _connection.BotToken);
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            var responseBody = await MessageHttpResponseReader
                .ReadUtf8BodyAsync(response, operationCancellation.Token)
                .ConfigureAwait(false);
            var parsed = SlackApiResponse.Parse(responseBody);
            var accepted = response.IsSuccessStatusCode && parsed.IsValid && parsed.Ok && validateSuccess(parsed);
            var invalidSuccessEnvelope = parsed.IsValid && parsed.Ok && !accepted;
            var statusCode = (int)response.StatusCode;
            var correlationId = SlackHttpResponseSupport.ReadCorrelationId(response);
            var result = new SlackDeliveryResult {
                DeliveryMethod = SlackDeliveryMethod.WebApi,
                Target = safeTarget,
                IsSuccess = accepted,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                ProviderCode = !parsed.IsValid || invalidSuccessEnvelope ? "invalid_response" : parsed.Error,
                CorrelationId = correlationId,
                RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
            };

            if (accepted) {
                result.Reference = createReference(parsed, correlationId);
                return result;
            }

            result.ErrorKind = invalidSuccessEnvelope || (!parsed.IsValid && response.IsSuccessStatusCode)
                ? MessageErrorKind.Transient
                : SlackHttpResponseSupport.Classify(statusCode, result.ProviderCode);
            result.ErrorMessage = result.ProviderCode is null
                ? $"Slack Web API {method} returned HTTP status {statusCode}."
                : $"Slack Web API rejected {method} with '{result.ProviderCode}'.";
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException($"Slack Web API {method} request timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException($"Slack Web API {method} request failed.", MessageErrorKind.Transient);
        }
    }
}
