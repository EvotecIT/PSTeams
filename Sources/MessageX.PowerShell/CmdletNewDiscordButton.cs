using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord interactive or link button.</summary>
/// <example>
/// <summary>Create an interactive approval button</summary>
/// <code>New-DiscordButton -Label 'Approve' -CustomId 'approve' -Style Success</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordButton", DefaultParameterSetName = "Interactive")]
[OutputType(typeof(DiscordButton))]
public sealed class CmdletNewDiscordButton : PSCmdlet {
    /// <summary>User-visible label.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Application-defined identifier.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Interactive")]
    public string CustomId { get; set; } = string.Empty;

    /// <summary>Interactive button style.</summary>
    [Parameter(ParameterSetName = "Interactive")]
    public DiscordButtonStyle Style { get; set; } = DiscordButtonStyle.Primary;

    /// <summary>External HTTPS URL for a link button.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Link")]
    public Uri? Url { get; set; }

    /// <summary>Creates a disabled button.</summary>
    [Parameter]
    public SwitchParameter Disabled { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordButton {
            Label = Label,
            CustomId = ParameterSetName == "Interactive" ? CustomId : null,
            Style = ParameterSetName == "Link" ? DiscordButtonStyle.Link : Style,
            Url = ParameterSetName == "Link" ? Url : null,
            Disabled = Disabled.IsPresent
        });
    }
}
