namespace MessageX.Core;

/// <summary>Provider-neutral conversation shapes retained with durable message coordinates.</summary>
public enum MessageConversationKind {
    /// <summary>The provider did not identify a more specific conversation shape.</summary>
    Unknown,

    /// <summary>A channel or equivalent shared conversation.</summary>
    Channel,

    /// <summary>A direct or private conversation.</summary>
    DirectMessage,

    /// <summary>A thread or reply-chain conversation.</summary>
    Thread
}
