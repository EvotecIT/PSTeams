namespace MessageX.Core;

/// <summary>
/// Operations that a provider connection, target, or message reference can support.
/// </summary>
[Flags]
public enum MessageCapabilities : long {
    /// <summary>No messaging operations are available.</summary>
    None = 0,
    /// <summary>Send a new message.</summary>
    Send = 1L << 0,
    /// <summary>Reply to a message or conversation thread.</summary>
    Reply = 1L << 1,
    /// <summary>Update an application-owned message.</summary>
    Update = 1L << 2,
    /// <summary>Delete an application-owned message.</summary>
    Delete = 1L << 3,
    /// <summary>Add or remove a reaction.</summary>
    React = 1L << 4,
    /// <summary>Upload or attach a file.</summary>
    UploadFile = 1L << 5,
    /// <summary>Receive provider events.</summary>
    ReceiveEvents = 1L << 6,
    /// <summary>Receive interactive actions or commands.</summary>
    ReceiveInteractions = 1L << 7,
    /// <summary>Maintain a provider realtime connection.</summary>
    Realtime = 1L << 8
}
