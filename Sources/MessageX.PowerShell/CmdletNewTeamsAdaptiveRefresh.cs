using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Creates an Adaptive Card refresh contract backed by Action.Execute.</summary>
/// <example>
/// <summary>Create a refresh action scoped to one Teams user</summary>
/// <code>$action = New-TeamsAdaptiveExecuteAction -Title 'Refresh' -Verb 'refresh'; New-TeamsAdaptiveRefresh -Action $action -UserId '29:example-user-id'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "TeamsAdaptiveRefresh")]
[OutputType(typeof(TeamsAdaptiveRefresh))]
public sealed class CmdletNewTeamsAdaptiveRefresh : PSCmdlet {
    /// <summary>Refresh action.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public TeamsAdaptiveExecuteAction Action { get; set; } = null!;

    /// <summary>Optional Teams user identifiers that receive automatic refreshes.</summary>
    [Parameter]
    public string[] UserId { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var refresh = new TeamsAdaptiveRefresh { Action = Action };
        foreach (var userId in UserId ?? Array.Empty<string>()) {
            refresh.UserIds.Add(userId);
        }
        WriteObject(refresh);
    }
}
