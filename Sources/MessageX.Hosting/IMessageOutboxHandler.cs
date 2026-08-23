namespace MessageX.Hosting;

/// <summary>Delivers one safe provider-owned outbox payload with caller-controlled retry semantics.</summary>
public interface IMessageOutboxHandler {
    /// <summary>Stable payload discriminator owned by this handler.</summary>
    string PayloadType { get; }

    /// <summary>Delivers one leased outbound operation.</summary>
    /// <remarks>
    /// Failures are treated as ambiguous and dead-lettered unless the handler throws
    /// <see cref="MessageOutboxDeliveryException"/> with
    /// <see cref="MessageOutboxDeliveryOutcome.DefinitelyNotSent"/>.
    /// </remarks>
    Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken);
}
