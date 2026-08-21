using System.Net;
using System.Net.Http;

namespace MessageX.Core;

/// <summary>
/// Creates MessageX-owned HTTP clients with conservative redirect, proxy, timeout, and user-agent behavior.
/// </summary>
public static class MessageHttpClientFactory {
    /// <summary>Creates an HTTP client and owned handler from the supplied transport options.</summary>
    public static HttpClient CreateClient(MessageHttpTransportOptions? options = null) {
        options ??= new MessageHttpTransportOptions();
        Validate(options);

        var client = new HttpClient(CreateHandler(options), disposeHandler: true) {
            Timeout = options.Timeout
        };
        var userAgent = options.UserAgent?.Trim();
        if (!string.IsNullOrWhiteSpace(userAgent)) {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        return client;
    }

    /// <summary>Creates an HTTP handler with automatic redirects disabled.</summary>
    public static HttpClientHandler CreateHandler(MessageHttpTransportOptions? options = null) {
        options ??= new MessageHttpTransportOptions();
        Validate(options);

        var handler = new HttpClientHandler {
            AllowAutoRedirect = false
        };
        if (options.ProxyUri is not null) {
            handler.Proxy = new WebProxy(options.ProxyUri);
            handler.UseProxy = true;
        }

        return handler;
    }

    /// <summary>
    /// Creates a linked cancellation source that applies the HTTP client timeout to the complete
    /// provider operation, including streamed response-body reads after headers arrive.
    /// </summary>
    public static CancellationTokenSource CreateOperationCancellation(
        HttpClient httpClient,
        CancellationToken cancellationToken = default) {
        if (httpClient is null) {
            throw new ArgumentNullException(nameof(httpClient));
        }

        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (httpClient.Timeout != Timeout.InfiniteTimeSpan) {
            source.CancelAfter(httpClient.Timeout);
        }
        return source;
    }

    private static void Validate(MessageHttpTransportOptions options) {
        if (options.Timeout <= TimeSpan.Zero && options.Timeout != Timeout.InfiniteTimeSpan) {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be positive or infinite.");
        }

        if (options.ProxyUri is not null &&
            (!options.ProxyUri.IsAbsoluteUri ||
             (options.ProxyUri.Scheme != Uri.UriSchemeHttp && options.ProxyUri.Scheme != Uri.UriSchemeHttps))) {
            throw new ArgumentException("Proxy URI must be an absolute HTTP or HTTPS URI.", nameof(options));
        }
    }
}
