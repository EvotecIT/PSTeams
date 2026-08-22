using Microsoft.AspNetCore.Http;

namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Thin ASP.NET Core adapter over the host-neutral Discord interaction receiver.</summary>
public sealed class DiscordHttpEndpointHandler {
    private readonly MessageInboundRequestReader _reader;
    private readonly MessageReceiveResultProcessor _processor;

    /// <summary>Creates a Discord endpoint handler.</summary>
    public DiscordHttpEndpointHandler(
        MessageInboundRequestReader reader,
        MessageReceiveResultProcessor processor) {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    /// <summary>Receives one Discord HTTP interaction.</summary>
    public async Task HandleAsync(
        HttpContext context,
        DiscordEndpointConfiguration configuration,
        CancellationToken cancellationToken = default) {
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
            var result = DiscordInteractionReceiver.Receive(
                request,
                configuration.PublicKeyHex,
                context.Request.Headers["X-Signature-Ed25519"].ToString(),
                context.Request.Headers["X-Signature-Timestamp"].ToString(),
                configuration.ReplayWindow);
            await _processor.ProcessAsync(context.Response, result, operationToken).ConfigureAwait(false);
        } catch (MessageInboundBodyTooLargeException) {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentLength = 0;
        }
    }
}
