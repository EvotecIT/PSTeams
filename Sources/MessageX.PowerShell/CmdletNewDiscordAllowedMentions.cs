using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates an explicit Discord mention policy. The default policy notifies nobody.</summary>
[Cmdlet(VerbsCommon.New, "DiscordAllowedMentions")]
[OutputType(typeof(DiscordAllowedMentions))]
public sealed class CmdletNewDiscordAllowedMentions : PSCmdlet {
    /// <summary>Parses user mention syntax in message content.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter ParseUsers { get; set; }

    /// <summary>Parses role mention syntax in message content.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter ParseRoles { get; set; }

    /// <summary>Parses everyone and here mention syntax.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter ParseEveryone { get; set; }

    /// <summary>Explicit user identifiers that may receive mentions.</summary>
    [Parameter(Mandatory = false)]
    public string[] UserId { get; set; } = Array.Empty<string>();

    /// <summary>Explicit role identifiers that may receive mentions.</summary>
    [Parameter(Mandatory = false)]
    public string[] RoleId { get; set; } = Array.Empty<string>();

    /// <summary>Mentions the author of a replied-to message.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter RepliedUser { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var mentions = new DiscordAllowedMentions {
            ParseUsers = ParseUsers.IsPresent,
            ParseRoles = ParseRoles.IsPresent,
            ParseEveryone = ParseEveryone.IsPresent,
            RepliedUser = RepliedUser.IsPresent
        };
        foreach (var id in UserId ?? Array.Empty<string>()) {
            mentions.UserIds.Add(id);
        }
        foreach (var id in RoleId ?? Array.Empty<string>()) {
            mentions.RoleIds.Add(id);
        }
        WriteObject(mentions);
    }
}
