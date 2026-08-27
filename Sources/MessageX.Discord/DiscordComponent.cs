namespace MessageX.Discord;

/// <summary>A provider-native Discord message or modal component.</summary>
public abstract class DiscordComponent {
    /// <summary>Discord component type identifier.</summary>
    public abstract int Type { get; }
}

/// <summary>A Discord component that can be placed inside an action row.</summary>
public abstract class DiscordInteractiveComponent : DiscordComponent {
}
