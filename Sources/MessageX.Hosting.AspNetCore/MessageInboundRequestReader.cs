using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Reads exact bounded request bytes without retaining headers or provider credentials.</summary>
public sealed class MessageInboundRequestReader {
    private readonly int _maximumBodyBytes;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates an exact request reader from validated host options.</summary>
    public MessageInboundRequestReader(
        IOptions<MessageXHostingAspNetCoreOptions> options,
        TimeProvider timeProvider) {
        ArgumentNullException.ThrowIfNull(options);
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _maximumBodyBytes = options.Value.MaximumRequestBodyBytes;
    }

    /// <summary>Reads one request for a route-selected installation.</summary>
    public async Task<MessageInboundRequest> ReadAsync(
        HttpRequest request,
        string installationId,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ContentLength is > 0 && request.ContentLength > _maximumBodyBytes) {
            throw new MessageInboundBodyTooLargeException(_maximumBodyBytes);
        }

        using var buffer = new MemoryStream(Math.Min(
            request.ContentLength is > 0 ? checked((int)request.ContentLength.Value) : 0,
            _maximumBodyBytes));
        var chunk = new byte[Math.Min(81920, _maximumBodyBytes + 1)];
        while (true) {
            var read = await request.Body.ReadAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (read == 0) {
                break;
            }
            if (buffer.Length + read > _maximumBodyBytes) {
                throw new MessageInboundBodyTooLargeException(_maximumBodyBytes);
            }
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        return new MessageInboundRequest(
            installationId,
            request.ContentType ?? "application/octet-stream",
            buffer.ToArray(),
            _timeProvider.GetUtcNow());
    }
}
