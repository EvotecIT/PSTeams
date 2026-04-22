using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommunications.Send, "TeamsMessage", SupportsShouldProcess = true)]
[Alias("TeamsMessage")]
[OutputType(typeof(TeamsDeliveryResult), typeof(string))]
public sealed class CmdletSendTeamsMessage : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedMessage")]
    public TeamsMessageRequest Message { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedHeroCard")]
    public TeamsHeroCard HeroCard { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedThumbnailCard")]
    public TeamsThumbnailCard ThumbnailCard { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedListCard")]
    public TeamsListCard ListCard { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedMessage")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedHeroCard")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedThumbnailCard")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedListCard")]
    public TeamsMessageTarget Target { get; set; } = null!;

    [Parameter(Mandatory = false, ParameterSetName = "TypedMessage")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedHeroCard")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedThumbnailCard")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedListCard")]
    public SwitchParameter PassThru { get; set; }

    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "LegacySections")]
    public ScriptBlock? SectionsInput { get; set; }

    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = true, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = true, ParameterSetName = "LegacySections")]
    public Uri Uri { get; set; } = null!;

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageTitle { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageText { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageSummary { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? Color { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public SwitchParameter HideOriginalBody { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public Uri? Proxy { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "LegacySections")]
    public TeamsMessageSection[] Sections { get; set; } = Array.Empty<TeamsMessageSection>();

    [Alias("Supress")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public bool Suppress { get; set; } = true;

    protected override void ProcessRecord() {
        if (ParameterSetName.StartsWith("Typed", StringComparison.Ordinal)) {
            ProcessTypedRecord();
            return;
        }

        ProcessLegacyRecord();
    }

    private void ProcessTypedRecord() {
        if (!ShouldProcess(GetShouldProcessTarget(), $"Send {GetTypedPayloadName()} using {Target.DeliveryMethod}")) {
            return;
        }

        var result = ParameterSetName switch {
            "TypedHeroCard" => TeamsClient.Default.SendAsync(HeroCard, Target).GetAwaiter().GetResult(),
            "TypedThumbnailCard" => TeamsClient.Default.SendAsync(ThumbnailCard, Target).GetAwaiter().GetResult(),
            "TypedListCard" => TeamsClient.Default.SendAsync(ListCard, Target).GetAwaiter().GetResult(),
            _ => TeamsClient.Default.SendAsync(Message, Target).GetAwaiter().GetResult()
        };

        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-TeamsMessage"));
        }

        if (PassThru) {
            WriteObject(result);
        }
    }

    private void ProcessLegacyRecord() {
        var request = new TeamsMessageRequest {
            Title = MessageTitle,
            Text = MessageText,
            Summary = MessageSummary,
            ThemeColor = ResolveThemeColor(),
            HideOriginalBody = HideOriginalBody.IsPresent,
            UseConnectorCardFormat = true
        };

        foreach (var section in ResolveLegacySections()) {
            request.Sections.Add(section);
        }

        var renderedBody = WebhookMessageRenderer.Render(request);
        WriteVerbose($"Send-TeamsMessage - Body {renderedBody}");

        if (!ShouldProcess(Uri.Host, "Send Teams message using IncomingWebhook")) {
            if (!Suppress) {
                WriteObject(renderedBody);
            }

            return;
        }

        var client = TeamsPowerShellDeliverySupport.CreateClient(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = client.SendAsync(request, target).GetAwaiter().GetResult();

        WriteVerbose($"Send-TeamsMessage - Execute {result.ResponseBody}");
        TeamsPowerShellDeliverySupport.WriteDeliveryIssue(this, result, "Send-TeamsMessage");

        if (!Suppress) {
            WriteObject(renderedBody);
        }
    }

    private IEnumerable<TeamsMessageSection> ResolveLegacySections() {
        if (string.Equals(ParameterSetName, "LegacySections", StringComparison.Ordinal)) {
            return Sections ?? Array.Empty<TeamsMessageSection>();
        }

        if (SectionsInput is null) {
            return Array.Empty<TeamsMessageSection>();
        }

        return SectionsInput
            .Invoke()
            .Select(item => item?.BaseObject)
            .OfType<TeamsMessageSection>()
            .ToArray();
    }

    private string? ResolveThemeColor() {
        if (string.IsNullOrWhiteSpace(Color)) {
            return null;
        }

        try {
            return TeamsColorUtility.NormalizeToHex(Color);
        } catch (ArgumentException exception) {
            var errorMessage = exception.Message.Replace(Environment.NewLine, " ");
            WriteWarning($"Send-TeamsMessage - Color conversion for {Color} failed. Error message: {errorMessage}");
            return null;
        }
    }

    private string GetShouldProcessTarget() {
        if (!string.IsNullOrWhiteSpace(Target.DisplayName)) {
            return Target.DisplayName!;
        }

        return $"{Target.DeliveryMethod} target at {Target.TargetUri.Host}";
    }

    private string GetTypedPayloadName() {
        return ParameterSetName switch {
            "TypedHeroCard" => "Teams HeroCard",
            "TypedThumbnailCard" => "Teams ThumbnailCard",
            "TypedListCard" => "Teams ListCard",
            _ => "Teams message"
        };
    }
}
