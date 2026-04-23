using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a connector-card button/action.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsButton")]
[Alias("TeamsButton")]
[OutputType(typeof(TeamsMessageButton))]
public sealed class CmdletNewTeamsButton : PSCmdlet {
    [Alias("ButtonName")]
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNull]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; } = null!;

    [Alias("TargetUri", "Uri", "Url")]
    [Parameter(Mandatory = true, Position = 1)]
    [ValidateNotNull]
    [ValidateNotNullOrEmpty]
    public string Link { get; set; } = null!;

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
