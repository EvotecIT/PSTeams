namespace MessageX.Hosting;

/// <summary>Result of atomically accepting or recognizing one inbox item.</summary>
public sealed class MessageDurableAcceptance {
    /// <summary>Creates one acceptance result.</summary>
    public MessageDurableAcceptance(string recordId, MessageDurableAcceptanceStatus status) {
        RecordId = MessageDurableValidation.RequiredOpaque(recordId, nameof(recordId));
        Status = status;
    }

    /// <summary>Stable storage record identifier.</summary>
    public string RecordId { get; }

    /// <summary>Idempotent acceptance state.</summary>
    public MessageDurableAcceptanceStatus Status { get; }
}
