namespace MessageX.Hosting;

/// <summary>Delivers one safe provider-owned outbox payload with caller-controlled retry semantics.</summary>
public interface IMessageOutboxHandler {
    /// <summary>Stable payload discriminator owned by this handler.</summary>
    string PayloadType { get; }

    /// <summary>Delivers one leased outbound operation. Throw to request a bounded retry.</summary>
    Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken);
}
