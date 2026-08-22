namespace MessageX.Hosting;

/// <summary>Transient exact-body request selected for one trusted installation route.</summary>
public sealed class MessageInboundRequest {
    private const int MaximumContentTypeLength = 256;
    private readonly byte[] _body;
    private string? _correlationId;

    /// <summary>Creates a host-neutral inbound request without retaining provider headers or credentials.</summary>
    public MessageInboundRequest(
        string installationId,
        string contentType,
        byte[] body,
        DateTimeOffset receivedAt) {
        InstallationId = NormalizeRequired(installationId, nameof(installationId), 256);
        ContentType = NormalizeRequired(contentType, nameof(contentType), MaximumContentTypeLength);
        if (body is null) {
            throw new ArgumentNullException(nameof(body));
        }
        _body = (byte[])body.Clone();
        ReceivedAt = receivedAt;
    }

    /// <summary>Non-secret installation identifier selected by the trusted HTTP route.</summary>
    public string InstallationId { get; }

    /// <summary>Original request content type.</summary>
    public string ContentType { get; }

    /// <summary>Exact raw request-body length.</summary>
    public int BodyLength => _body.Length;

    /// <summary>Time at which the host received the request.</summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>Safe transport correlation identifier, when supplied by the host.</summary>
    public string? CorrelationId {
        get => _correlationId;
        set => _correlationId = MessageDiagnosticToken.Normalize(value);
    }

    /// <summary>Returns an independent copy of the exact raw request body.</summary>
    public byte[] CopyBody() => (byte[])_body.Clone();

    private static string NormalizeRequired(string? value, string parameterName, int maximumLength) {
        if (value is not null && (value.Length > maximumLength || value.Any(char.IsControl))) {
            throw new ArgumentException(
                "Inbound request coordinates must be bounded text without control characters.",
                parameterName);
        }
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized)) {
            throw new ArgumentException(
                "Inbound request coordinates must be bounded text without control characters.",
                parameterName);
        }
        return normalized!;
    }
}
