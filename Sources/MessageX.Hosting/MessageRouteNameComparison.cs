namespace MessageX.Hosting;

/// <summary>Comparison semantics for a provider route's normalized name.</summary>
public enum MessageRouteNameComparison {
    /// <summary>The route has no name.</summary>
    None = 0,

    /// <summary>The provider identifier is opaque and must match exactly.</summary>
    Ordinal = 1,

    /// <summary>The provider name is case-insensitive.</summary>
    OrdinalIgnoreCase = 2
}
