namespace MessageX.Hosting;

/// <summary>Store-authoritative result of renewing one owned durable lease.</summary>
public sealed class MessageLeaseRenewal {
    /// <summary>Creates a successful lease-renewal result.</summary>
    public MessageLeaseRenewal(DateTimeOffset leaseExpiresAt) : this(leaseExpiresAt, null) {
    }

    /// <summary>Creates a successful lease-renewal result with a store-authoritative relative duration.</summary>
    public MessageLeaseRenewal(DateTimeOffset leaseExpiresAt, TimeSpan? leaseDuration) {
        if (leaseDuration.HasValue && leaseDuration.Value <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        }
        LeaseExpiresAt = leaseExpiresAt;
        LeaseDuration = leaseDuration;
    }

    /// <summary>New expiration in the store's authoritative UTC clock.</summary>
    public DateTimeOffset LeaseExpiresAt { get; }

    /// <summary>
    /// Store-authoritative relative lease duration, when supplied. Workers use this value for local renewal timing
    /// without comparing clocks from different systems.
    /// </summary>
    public TimeSpan? LeaseDuration { get; }
}
