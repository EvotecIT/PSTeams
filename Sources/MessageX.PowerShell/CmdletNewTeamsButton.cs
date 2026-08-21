using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a connector-card button/action.
/// </summary>
/// <example>
/// <summary>Create a button that opens a build page</summary>
/// <code>New-TeamsButton -Name 'Open build' -Link 'https://ci.example.test/build/42' -Type OpenUri</code>
/// </example>
[Cmdlet(VerbsCommon.New, "TeamsButton")]
[Alias("TeamsButton")]
[OutputType(typeof(TeamsMessageButton))]
public sealed class CmdletNewTeamsButton : PSCmdlet {
    /// <summary>Text displayed on the button.</summary>
    [Alias("ButtonName")]
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNull]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; } = null!;

    /// <summary>Target URL or action value used when the button is selected.</summary>
    [Alias("TargetUri", "Uri", "Url")]
    [Parameter(Mandatory = true, Position = 1)]
    [ValidateNotNull]
    [ValidateNotNullOrEmpty]
    public string Link { get; set; } = null!;

    /// <summary>Connector-card action type. The default is ViewAction.</summary>
    [Alias("ButtonType")]
    [Parameter(Mandatory = false)]
    public TeamsMessageButtonType Type { get; set; } = TeamsMessageButtonType.ViewAction;

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageButton {
            Name = Name,
            Link = Link,
            ButtonType = Type
        });
    }
}
