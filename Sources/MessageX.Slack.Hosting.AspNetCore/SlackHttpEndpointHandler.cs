using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace MessageX.Slack.Hosting.AspNetCore;

/// <summary>Thin ASP.NET Core adapter over the host-neutral Slack receivers.</summary>
public sealed class SlackHttpEndpointHandler {
    private readonly MessageInboundRequestReader _reader;
    private readonly MessageReceiveResultProcessor _processor;

    /// <summary>Creates a Slack endpoint handler.</summary>
    public SlackHttpEndpointHandler(
        MessageInboundRequestReader reader,
        MessageReceiveResultProcessor processor) {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    /// <summary>Receives one Slack Events API request.</summary>
    public Task HandleEventsAsync(
        HttpContext context,
        SlackEndpointConfiguration configuration,
        CancellationToken cancellationToken = default) =>
        HandleAsync(context, configuration, eventsApi: true, cancellationToken);

    /// <summary>Receives one Slack slash-command or interaction request.</summary>
    public Task HandleInteractionsAsync(
        HttpContext context,
        SlackEndpointConfiguration configuration,
        CancellationToken cancellationToken = default) =>
        HandleAsync(context, configuration, eventsApi: false, cancellationToken);

    private async Task HandleAsync(
        HttpContext context,
        SlackEndpointConfiguration configuration,
        bool eventsApi,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configuration);
        using var operationCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted,
            cancellationToken);
        var operationToken = operationCancellation.Token;
        try {
            var request = await _reader.ReadAsync(
                context.Request,
                configuration.InstallationId,
                operationToken).ConfigureAwait(false);
            request.CorrelationId = context.TraceIdentifier;
            var signature = context.Request.Headers["X-Slack-Signature"].ToString();
            var timestamp = context.Request.Headers["X-Slack-Request-Timestamp"].ToString();
            if (eventsApi) {
                var retryNumber = ParseRetryNumber(context.Request.Headers["X-Slack-Retry-Num"].ToString());
                var retryReason = OptionalHeader(context, "X-Slack-Retry-Reason");
                var result = SlackEventsApiReceiver.Receive(
                    request,
                    configuration.SigningSecret,
                    signature,
                    timestamp,
                    retryNumber,
                    retryReason,
                    configuration.ReplayWindow,
                    configuration.Identity);
                await _processor.ProcessAsync(context.Response, result, operationToken).ConfigureAwait(false);
            } else {
                var result = SlackInteractionReceiver.Receive(
                    request,
                    configuration.SigningSecret,
                    signature,
                    timestamp,
                    configuration.ReplayWindow,
                    configuration.Identity);
                await _processor.ProcessAsync(context.Response, result, operationToken).ConfigureAwait(false);
            }
        } catch (MessageInboundBodyTooLargeException) {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentLength = 0;
        } catch (ArgumentException exception) when (
            string.Equals(exception.ParamName, "contentType", StringComparison.Ordinal)) {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            context.Response.ContentLength = 0;
        }
    }

    private static int? ParseRetryNumber(string value) {
        if (string.IsNullOrEmpty(value)) {
            return null;
        }
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var retry)
            ? retry
            : -1;
    }

    private static string? OptionalHeader(HttpContext context, string name) {
        var value = context.Request.Headers[name].ToString();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
