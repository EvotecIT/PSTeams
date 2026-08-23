namespace MessageX.Hosting;

/// <summary>Transient provider acknowledgement returned by an HTTP host.</summary>
public sealed class MessageAcknowledgement {
    /// <summary>Largest provider acknowledgement body retained or written by MessageX.</summary>
    public const int MaximumBodyBytes = 64 * 1024;

    private readonly byte[] _body;

    /// <summary>Creates an acknowledgement with an exact response body.</summary>
    public MessageAcknowledgement(int statusCode, string? contentType, byte[]? body = null) {
        if (statusCode is < 100 or > 599) {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }
        if (contentType is not null &&
            (contentType.Length > 256 || contentType.Any(char.IsControl))) {
            throw new ArgumentException(
                "Acknowledgement content types must be bounded text without control characters.",
                nameof(contentType));
        }
        if (body is { Length: > MaximumBodyBytes }) {
            throw new ArgumentException(
                $"Acknowledgement bodies cannot exceed {MaximumBodyBytes} bytes.",
                nameof(body));
        }
        StatusCode = statusCode;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType!.Trim();
        _body = body is null ? Array.Empty<byte>() : (byte[])body.Clone();
    }

    /// <summary>HTTP status code to return to the provider.</summary>
    public int StatusCode { get; }

    /// <summary>Response content type, when a body is present.</summary>
    public string? ContentType { get; }

    /// <summary>Exact acknowledgement-body length.</summary>
    public int BodyLength => _body.Length;

    /// <summary>Returns an independent copy of the acknowledgement body.</summary>
    public byte[] CopyBody() => (byte[])_body.Clone();

    /// <summary>Creates an acknowledgement without a response body.</summary>
    public static MessageAcknowledgement Empty(int statusCode) => new(statusCode, null);
}
