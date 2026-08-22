namespace MessageX.Slack;

/// <summary>Outcome of verifying the signature and replay window of a Slack HTTP request.</summary>
public enum SlackRequestVerificationResult {
    /// <summary>The signature or timestamp is malformed or does not authenticate the exact body.</summary>
    Invalid = 0,

    /// <summary>The signature authenticates the exact body and the timestamp is inside the replay window.</summary>
    Valid = 1,

    /// <summary>The signature authenticates the exact body but the timestamp is outside the replay window.</summary>
    Stale = 2
}
