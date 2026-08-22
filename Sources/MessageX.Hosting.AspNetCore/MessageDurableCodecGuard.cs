namespace MessageX.Hosting.AspNetCore;

internal static class MessageDurableCodecGuard {
    public static void ValidateEncoded<TProviderPayload>(
        MessageDurableRecord record,
        MessageRoute route,
        MessageEventEnvelope<TProviderPayload> envelope,
        string payloadType) {
        ArgumentNullException.ThrowIfNull(record);
        if (!string.Equals(record.PayloadType, payloadType, StringComparison.Ordinal) ||
            !string.Equals(record.Provider, envelope.Provider, StringComparison.Ordinal) ||
            !string.Equals(record.InstallationId, envelope.InstallationId, StringComparison.Ordinal) ||
            !string.Equals(record.DeduplicationKey, envelope.DeduplicationKey, StringComparison.Ordinal) ||
            record.ReceivedAt != envelope.ReceivedAt ||
            !RoutesMatch(record.Route, route)) {
            throw new InvalidOperationException("The durable codec changed verified routing coordinates.");
        }
    }

    public static void ValidateDecoded<TProviderPayload>(
        MessageDurableRecord record,
        MessageEventEnvelope<TProviderPayload> envelope) {
        ArgumentNullException.ThrowIfNull(envelope);
        if (!string.Equals(record.Provider, envelope.Provider, StringComparison.Ordinal) ||
            !string.Equals(record.InstallationId, envelope.InstallationId, StringComparison.Ordinal) ||
            !string.Equals(record.DeduplicationKey, envelope.DeduplicationKey, StringComparison.Ordinal) ||
            record.ReceivedAt != envelope.ReceivedAt ||
            record.Route.EventKind != envelope.Kind) {
            throw new MessageDurablePayloadException(
                "The decoded durable payload does not match its verified routing coordinates.");
        }
    }

    private static bool RoutesMatch(MessageRoute left, MessageRoute right) =>
        left.Kind == right.Kind &&
        left.EventKind == right.EventKind &&
        string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
}
