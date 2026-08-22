using System.Net.Http;
using System.Text;

namespace MessageX.Core;

/// <summary>Reads provider HTTP response bodies with a strict memory bound.</summary>
public static class MessageHttpResponseReader {
    /// <summary>Default maximum provider response size.</summary>
    public const int DefaultMaximumBytes = 64 * 1024;

    /// <summary>
    /// Reads a UTF-8 response body up to the configured limit. An empty value is returned when the
    /// declared or streamed body exceeds the limit.
    /// </summary>
    public static async Task<string> ReadUtf8BodyAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default,
        int maximumBytes = DefaultMaximumBytes) {
        if (response is null) {
            throw new ArgumentNullException(nameof(response));
        }
        if (maximumBytes <= 0) {
            throw new ArgumentOutOfRangeException(nameof(maximumBytes), "The response limit must be positive.");
        }
        if (response.Content is null) {
            return string.Empty;
        }
        if (response.Content.Headers.ContentLength > maximumBytes) {
            return string.Empty;
        }

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[Math.Min(4096, maximumBytes)];
        while (buffer.Length <= maximumBytes) {
            var count = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false);
            if (count == 0) {
                break;
            }
            if (buffer.Length + count > maximumBytes) {
                return string.Empty;
            }
            buffer.Write(chunk, 0, count);
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
