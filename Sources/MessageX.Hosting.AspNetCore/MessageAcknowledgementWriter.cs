using Microsoft.AspNetCore.Http;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Writes exact provider acknowledgements without JSON reserialization.</summary>
public sealed class MessageAcknowledgementWriter {
    /// <summary>Writes one acknowledgement to an ASP.NET Core response.</summary>
    public async Task WriteAsync(
        HttpResponse response,
        MessageAcknowledgement acknowledgement,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(acknowledgement);

        response.StatusCode = acknowledgement.StatusCode;
        response.ContentLength = acknowledgement.BodyLength;
        response.ContentType = acknowledgement.ContentType;
        var body = acknowledgement.CopyBody();
        if (body.Length > 0) {
            await response.Body.WriteAsync(body.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }
}
