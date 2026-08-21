using System.IO;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a typed activity-image directive for connector-card sections.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsActivityImage", DefaultParameterSetName = "Link")]
[Alias("ActivityImageLink", "TeamsActivityImageLink", "New-TeamsActivityImageLink", "ActivityImage", "TeamsActivityImage")]
[OutputType(typeof(TeamsMessageSectionDirective))]
public sealed class CmdletNewTeamsActivityImage : PSCmdlet {
    [Parameter(Mandatory = false, ParameterSetName = "Image")]
    [ValidateSet("Add", "Alert", "Cancel", "Check", "Disable", "Download", "Info", "Minus", "Question", "Reload", "None")]
    public string? Image { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "Link")]
    public string? Link { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "Path")]
    public FileInfo? Path { get; set; }

    protected override void ProcessRecord() {
        var value = ResolveValue();
        if (value is null) {
            return;
        }

        WriteObject(new TeamsMessageSectionDirective {
            DirectiveType = TeamsMessageSectionDirectiveType.ActivityImage,
            Value = value
        });
    }

    private string? ResolveValue() {
        if (ParameterSetName == "Path") {
            if (Path is null) {
                return null;
            }

            TeamsPowerShellImageSupport.ValidateImageFile(
                Path,
                nameof(Path),
                "Path is inaccessible or does not exist",
                "Path is not a file or file extension is not supported");

            return TeamsPowerShellImageSupport.ResolveImageFile(Path);
        }

        if (ParameterSetName == "Image") {
            if (string.Equals(Image, "None", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            return string.IsNullOrWhiteSpace(Image)
                ? null
                : TeamsPowerShellImageSupport.ResolveBuiltInImage(Image!);
        }

        return Link;
    }
}
