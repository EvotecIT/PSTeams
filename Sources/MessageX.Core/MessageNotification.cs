namespace MessageX;

/// <summary>
/// The product-owned notification data that provider renderers will project into native messages.
/// It contains no provider styling or delivery credentials.
/// </summary>
public sealed class MessageNotification {
    public MessageNotificationSeverity Severity { get; }
    public string Title { get; }
    public string Explanation { get; }
    public IReadOnlyList<MessageNotificationFact> Facts { get; }
    public string IncidentId { get; }
    public Uri ReportUrl { get; }
    public string PlainTextFallback { get; }

    public MessageNotification(MessageNotificationSeverity severity, string title, string explanation,
        IEnumerable<MessageNotificationFact> facts, string incidentId, Uri reportUrl, string plainTextFallback) {
        if (!Enum.IsDefined(typeof(MessageNotificationSeverity), severity)) {
            throw new ArgumentOutOfRangeException(nameof(severity));
        }
        Severity = severity;
        Title = Require(title, nameof(title));
        Explanation = Require(explanation, nameof(explanation));
        IncidentId = Require(incidentId, nameof(incidentId));
        PlainTextFallback = Require(plainTextFallback, nameof(plainTextFallback));
        ReportUrl = reportUrl is { IsAbsoluteUri: true } && reportUrl.Scheme == Uri.UriSchemeHttps
            ? reportUrl
            : throw new ArgumentException("A report URL must be absolute and use HTTPS.", nameof(reportUrl));
        Facts = Array.AsReadOnly((facts ?? throw new ArgumentNullException(nameof(facts))).ToArray());
        if (Facts.Any(static fact => fact is null)) {
            throw new ArgumentException("Facts cannot contain null entries.", nameof(facts));
        }
    }

    private static string Require(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value;
}
