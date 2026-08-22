namespace MessageX.Hosting;

/// <summary>Host action selected after provider verification and parsing.</summary>
public enum MessageReceiveStatus {
    /// <summary>Return the acknowledgement but do not enqueue or dispatch.</summary>
    Rejected = 0,
    /// <summary>Acknowledge a valid request that does not require application dispatch.</summary>
    Acknowledged,
    /// <summary>Persist or enqueue the verified event before returning its acknowledgement.</summary>
    DispatchReady
}
