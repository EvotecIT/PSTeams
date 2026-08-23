using System.Text.Json;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Capability-free Teams attachment metadata and bounded content available after durable restoration.</summary>
public sealed class TeamsInboundAttachment {
    /// <summary>Creates a safe inbound attachment projection.</summary>
    public TeamsInboundAttachment(string? contentType, string? name, JsonElement? content) {
        ContentType = contentType;
        Name = name;
        Content = content?.Clone();
    }

    /// <summary>Provider attachment content type.</summary>
    public string? ContentType { get; }

    /// <summary>Provider attachment name, when supplied.</summary>
    public string? Name { get; }

    /// <summary>Bounded capability-free attachment content, including Adaptive Card bodies.</summary>
    public JsonElement? Content { get; }
}
