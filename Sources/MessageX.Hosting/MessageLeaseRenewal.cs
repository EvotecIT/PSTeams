namespace MessageX.Hosting;

/// <summary>Store-authoritative result of renewing one owned durable lease.</summary>
public sealed class MessageLeaseRenewal {
    /// <summary>Creates a successful lease-renewal result.</summary>
    public MessageLeaseRenewal(DateTimeOffset leaseExpiresAt) => LeaseExpiresAt = leaseExpiresAt;

    /// <summary>New authoritative lease expiration.</summary>
    public DateTimeOffset LeaseExpiresAt { get; }
}
