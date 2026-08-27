using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>Creates a Teams Universal Action with optional compatibility fallback.</summary>
/// <example>
/// <summary>Create an approval action with a submit fallback</summary>
/// <code>$fallback = New-TeamsAdaptiveSubmitAction -Title 'Approve' -Data @{ action = 'approve' }; New-TeamsAdaptiveExecuteAction -Title 'Approve' -Verb 'approve' -Data @{ incident = 'INC-42' } -Fallback $fallback</code>
/// </example>
[Cmdlet(VerbsCommon.New, "TeamsAdaptiveExecuteAction")]
[OutputType(typeof(TeamsAdaptiveExecuteAction))]
public sealed class CmdletNewTeamsAdaptiveExecuteAction : PSCmdlet {
    /// <summary>Button label.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Application route verb.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Verb { get; set; } = string.Empty;

    /// <summary>Optional action identifier.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>JSON-compatible action data.</summary>
    [Parameter]
    public IDictionary? Data { get; set; }

    /// <summary>Input association policy.</summary>
    [Parameter]
    [ValidateSet("Auto", "None")]
    public string AssociatedInputs { get; set; } = "Auto";

    /// <summary>Optional Action.Submit fallback for older clients.</summary>
    [Parameter]
    public TeamsAdaptiveSubmitAction? Fallback { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveExecuteAction {
            Id = Id,
            Title = Title,
            Verb = Verb,
            Data = PowerShellMessageDataValueConverter.FromDictionary(Data),
            AssociatedInputs = string.Equals(AssociatedInputs, "None", StringComparison.OrdinalIgnoreCase)
                ? TeamsAdaptiveAssociatedInputs.None
                : TeamsAdaptiveAssociatedInputs.Auto,
            Fallback = Fallback
        });
    }
}
