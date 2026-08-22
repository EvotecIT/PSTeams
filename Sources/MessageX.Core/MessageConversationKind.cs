namespace MessageX.Core;

/// <summary>Provider-neutral conversation shapes retained with durable message coordinates.</summary>
public enum MessageConversationKind {
    /// <summary>The provider did not identify a more specific conversation shape.</summary>
    Unknown = 0,

    /// <summary>A channel or equivalent shared conversation.</summary>
    Channel = 1,

    /// <summary>A direct or private conversation.</summary>
    DirectMessage = 2,

    /// <summary>A thread or reply-chain conversation.</summary>
    Thread = 3,

    /// <summary>A private group conversation with more than two participants.</summary>
    GroupChat = 4
}
