namespace MessageX.Core;

/// <summary>
/// Exposes operations supported by a provider connection, target, or reference.
/// </summary>
public interface IProviderCapabilities {
    /// <summary>Gets the supported operations.</summary>
    MessageCapabilities Capabilities { get; }
}
