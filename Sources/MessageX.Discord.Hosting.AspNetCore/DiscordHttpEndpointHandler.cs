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
        ArgumentNullException.ThrowIfNull(configuration);
        await HandleAsync(
            context,
            configuration.InstallationId,
            configuration.PublicKeyHex,
            configuration.ReplayWindow,
            configuration.ApplicationId,
            configuration.InstallationOwnerId,
            installationResolver: null,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Receives one Discord HTTP interaction on a shared application endpoint.</summary>
    public async Task HandleAsync(
        HttpContext context,
        DiscordApplicationEndpointConfiguration configuration,
        IDiscordInstallationResolver installationResolver,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(installationResolver);
        await HandleAsync(
            context,
            configuration.ApplicationId,
            configuration.PublicKeyHex,
            configuration.ReplayWindow,
            configuration.ApplicationId,
            expectedInstallationOwnerId: null,
            coordinates => installationResolver.ResolveInstallationId(coordinates),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleAsync(
        HttpContext context,
        string requestInstallationId,
        string publicKeyHex,
        TimeSpan replayWindow,
        string expectedApplicationId,
        string? expectedInstallationOwnerId,
        Func<DiscordInstallationContext, string?>? installationResolver,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(context);
        using var operationCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted,
            cancellationToken);
        var operationToken = operationCancellation.Token;
        try {
            var request = await _reader.ReadAsync(
                context.Request,
                requestInstallationId,
                operationToken).ConfigureAwait(false);
            request.CorrelationId = context.TraceIdentifier;
            var result = DiscordInteractionReceiver.Receive(
                request,
                publicKeyHex,
                context.Request.Headers["X-Signature-Ed25519"].ToString(),
                context.Request.Headers["X-Signature-Timestamp"].ToString(),
                replayWindow,
                expectedApplicationId,
                expectedInstallationOwnerId,
                installationResolver);
            await _processor.ProcessAsync(context.Response, result, operationToken).ConfigureAwait(false);
        } catch (MessageInboundBodyTooLargeException) {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentLength = 0;
        } catch (ArgumentException exception) when (
            string.Equals(exception.ParamName, "contentType", StringComparison.Ordinal)) {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            context.Response.ContentLength = 0;
        }
    }
}
