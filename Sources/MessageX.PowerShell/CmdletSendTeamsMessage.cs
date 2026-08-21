using System.Management.Automation;
using System.Collections;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Sends a typed or legacy-composed message to Microsoft Teams.
/// </summary>
/// <remarks>
/// Typed parameter sets accept MessageX.Teams message models and targets. Legacy parameter sets preserve the PSTeams composition syntax.
/// Workflows webhooks are the recommended notification transport for new automation.
/// </remarks>
/// <example>
/// <summary>Send a typed message through a Teams Workflows webhook</summary>
/// <code>$message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42'; $target = New-TeamsWebhookTarget -Uri $workflowUrl -Workflow; Send-TeamsMessage -Message $message -Target $target</code>
/// </example>
/// <example>
/// <summary>Preview a legacy connector-card payload without sending it</summary>
/// <code>Send-TeamsMessage -Uri $webhookUrl -MessageTitle 'Build failed' -MessageText 'Pipeline 42' -Suppress:$false -WhatIf</code>
/// </example>
[Cmdlet(VerbsCommunications.Send, "TeamsMessage", SupportsShouldProcess = true)]
[Alias("TeamsMessage")]
[OutputType(typeof(TeamsDeliveryResult), typeof(string))]
public sealed class CmdletSendTeamsMessage : TeamsWebhookCmdletBase {
    /// <summary>Typed Teams message request to send.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedMessage")]
    public TeamsMessageRequest Message { get; set; } = null!;

    /// <summary>Typed Teams HeroCard to send.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedHeroCard")]
    public TeamsHeroCard HeroCard { get; set; } = null!;

    /// <summary>Typed Teams ThumbnailCard to send.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedThumbnailCard")]
    public TeamsThumbnailCard ThumbnailCard { get; set; } = null!;

    /// <summary>Typed Teams ListCard to send.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "TypedListCard")]
    public TeamsListCard ListCard { get; set; } = null!;

    /// <summary>Typed Teams delivery target.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedMessage")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedHeroCard")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedThumbnailCard")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "TypedListCard")]
    public TeamsMessageTarget Target { get; set; } = null!;

    /// <summary>Returns the typed delivery result.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "TypedMessage")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedHeroCard")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedThumbnailCard")]
    [Parameter(Mandatory = false, ParameterSetName = "TypedListCard")]
    public SwitchParameter PassThru { get; set; }

    /// <summary>Legacy composition script block that emits Teams sections.</summary>
    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "LegacySections")]
    public ScriptBlock? SectionsInput { get; set; }

    /// <summary>HTTPS Teams incoming webhook URL used by the legacy parameter sets.</summary>
    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = true, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = true, ParameterSetName = "LegacySections")]
    public Uri Uri { get; set; } = null!;

    /// <summary>Legacy connector-card title.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageTitle { get; set; }

    /// <summary>Legacy connector-card body text.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageText { get; set; }

    /// <summary>Legacy connector-card summary used by notifications and accessibility clients.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? MessageSummary { get; set; }

    /// <summary>Theme color name or hexadecimal value for the legacy connector card.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public string? Color { get; set; }

    /// <summary>Requests Teams to hide the original message body when supported.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public SwitchParameter HideOriginalBody { get; set; }

    /// <summary>Pre-built connector-card sections for the legacy sections parameter set.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "LegacySections")]
    public TeamsMessageSection[] Sections { get; set; } = Array.Empty<TeamsMessageSection>();

    /// <summary>Suppresses the rendered legacy JSON output after processing. The default is true.</summary>
    [Alias("Supress")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacyScript")]
    [Parameter(Mandatory = false, ParameterSetName = "LegacySections")]
    public bool Suppress { get; set; } = true;

    protected override async Task ProcessRecordAsync() {
        if (ParameterSetName.StartsWith("Typed", StringComparison.Ordinal)) {
            await ProcessTypedRecordAsync();
            return;
        }

        await ProcessLegacyRecordAsync();
    }

    private async Task ProcessTypedRecordAsync() {
        if (!ShouldProcess(GetShouldProcessTarget(), $"Send {GetTypedPayloadName()} using {Target.DeliveryMethod}")) {
            return;
        }

        var result = await SendWithClientAsync(client => ParameterSetName switch {
            "TypedHeroCard" => client.SendAsync(HeroCard, Target, CancelToken),
            "TypedThumbnailCard" => client.SendAsync(ThumbnailCard, Target, CancelToken),
            "TypedListCard" => client.SendAsync(ListCard, Target, CancelToken),
            _ => client.SendAsync(Message, Target, CancelToken)
        });

        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-TeamsMessage"));
        }

        if (PassThru) {
            WriteObject(result);
        }
    }

    private async Task ProcessLegacyRecordAsync() {
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
        WriteVerbose($"Send-TeamsMessage - Prepared {renderedBody.Length} characters for {Uri.Host}.");

        if (!ShouldProcess(Uri.Host, "Send Teams message using IncomingWebhook")) {
            if (!Suppress) {
                WriteObject(renderedBody);
            }

            return;
        }

        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = await SendWithClientAsync(client => client.SendAsync(request, target, CancelToken));

        WriteVerbose($"Send-TeamsMessage - Completed with HTTP status {result.StatusCode?.ToString() ?? "unknown"}.");
        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-TeamsMessage"));
        }

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
            .Select(item => ConvertLegacySection(item?.BaseObject))
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

    private static TeamsMessageSection? ConvertLegacySection(object? value) {
        return value switch {
            null => null,
            TeamsMessageSection section => section,
            IDictionary dictionary => ConvertLegacySectionDictionary(dictionary),
            _ => null
        };
    }

    private static TeamsMessageSection ConvertLegacySectionDictionary(IDictionary dictionary) {
        var section = new TeamsMessageSection {
            Title = ReadString(dictionary, "title"),
            ActivityTitle = ReadString(dictionary, "activityTitle"),
            ActivitySubtitle = ReadString(dictionary, "activitySubtitle"),
            ActivityImage = ReadString(dictionary, "activityImage"),
            ActivityText = ReadString(dictionary, "activityText"),
            Text = ReadString(dictionary, "text"),
            StartGroup = ReadBool(dictionary, "startGroup")
        };

        foreach (var fact in ReadDictionaryArray(dictionary, "facts")) {
            section.Facts.Add(new TeamsMessageFact {
                Name = ReadString(fact, "name"),
                Value = ReadString(fact, "value")
            });
        }

        foreach (var action in ReadObjectArray(dictionary, "potentialAction")) {
            var button = ConvertLegacyButton(action);
            if (button is not null) {
                section.Buttons.Add(button);
            }
        }

        foreach (var image in ReadObjectArray(dictionary, "images")) {
            var imageUri = image switch {
                string value => value,
                IDictionary imageDictionary => ReadString(imageDictionary, "image"),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(imageUri)) {
                section.Images.Add(imageUri!);
            }
        }

        return section;
    }

    private static TeamsMessageButton? ConvertLegacyButton(object? value) {
        if (value is TeamsMessageButton button) {
            return button;
        }

        if (value is not IDictionary dictionary) {
            return null;
        }

        var buttonTypeName = ReadString(dictionary, "@type") ?? ReadString(dictionary, "type");
        var buttonType = ResolveLegacyButtonType(buttonTypeName, dictionary);
        var buttonLink = ResolveLegacyButtonLink(dictionary);

        return new TeamsMessageButton {
            Name = ReadString(dictionary, "name") ?? ReadString(dictionary, "Name"),
            Link = buttonLink,
            ButtonType = buttonType
        };
    }

    private static TeamsMessageButtonType ResolveLegacyButtonType(string? typeName, IDictionary dictionary) {
        if (string.Equals(typeName, "ActionCard", StringComparison.OrdinalIgnoreCase)) {
            var inputType = ReadNestedActionInputType(dictionary);
            if (string.Equals(inputType, "TextInput", StringComparison.OrdinalIgnoreCase)) {
                return TeamsMessageButtonType.TextInput;
            }

            if (string.Equals(inputType, "DateInput", StringComparison.OrdinalIgnoreCase)) {
                return TeamsMessageButtonType.DateInput;
            }

            return TeamsMessageButtonType.HttpPost;
        }

        return typeName?.ToUpperInvariant() switch {
            "VIEWACTION" => TeamsMessageButtonType.ViewAction,
            "HTTPPOST" => TeamsMessageButtonType.HttpPost,
            "OPENURI" => TeamsMessageButtonType.OpenUri,
            _ => TeamsMessageButtonType.ViewAction
        };
    }

    private static string? ReadNestedActionInputType(IDictionary dictionary) {
        foreach (var input in ReadObjectArray(dictionary, "Inputs")) {
            if (input is IDictionary inputDictionary) {
                return ReadString(inputDictionary, "@type");
            }
        }

        return null;
    }

    private static string? ResolveLegacyButtonLink(IDictionary dictionary) {
        var directTarget = ReadFirstString(dictionary, "target", "Target");
        if (!string.IsNullOrWhiteSpace(directTarget)) {
            return directTarget;
        }

        foreach (var target in ReadObjectArray(dictionary, "Targets")) {
            if (target is IDictionary targetDictionary) {
                var uri = ReadString(targetDictionary, "uri");
                if (!string.IsNullOrWhiteSpace(uri)) {
                    return uri;
                }
            }
        }

        foreach (var action in ReadObjectArray(dictionary, "actions")) {
            if (action is IDictionary actionDictionary) {
                var actionTarget = ReadFirstString(actionDictionary, "target", "Target");
                if (!string.IsNullOrWhiteSpace(actionTarget)) {
                    return actionTarget;
                }
            }
        }

        return null;
    }

    private static IEnumerable<IDictionary> ReadDictionaryArray(IDictionary dictionary, string key) {
        return ReadObjectArray(dictionary, key).OfType<IDictionary>();
    }

    private static IEnumerable<object> ReadObjectArray(IDictionary dictionary, string key) {
        if (!TryGetValue(dictionary, key, out var value) || value is null) {
            return Array.Empty<object>();
        }

        if (value is string) {
            return new object[] { value };
        }

        if (value is IEnumerable enumerable) {
            return enumerable.Cast<object>();
        }

        return new object[] { value };
    }

    private static string? ReadFirstString(IDictionary dictionary, params string[] keys) {
        foreach (var key in keys) {
            if (!TryGetValue(dictionary, key, out var value) || value is null) {
                continue;
            }

            if (value is string text) {
                return text;
            }

            if (value is IEnumerable enumerable and not string) {
                foreach (var item in enumerable) {
                    if (item is string itemText && !string.IsNullOrWhiteSpace(itemText)) {
                        return itemText;
                    }
                }
            }
        }

        return null;
    }

    private static string? ReadString(IDictionary dictionary, string key) {
        if (!TryGetValue(dictionary, key, out var value) || value is null) {
            return null;
        }

        return value switch {
            string text => text,
            _ => value.ToString()
        };
    }

    private static bool ReadBool(IDictionary dictionary, string key) {
        if (!TryGetValue(dictionary, key, out var value) || value is null) {
            return false;
        }

        return value switch {
            bool result => result,
            string text when bool.TryParse(text, out var parsed) => parsed,
            _ => false
        };
    }

    private static bool TryGetValue(IDictionary dictionary, string key, out object? value) {
        foreach (DictionaryEntry entry in dictionary) {
            if (string.Equals(entry.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase)) {
                value = entry.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}
