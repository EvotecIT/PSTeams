using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a connector-card section.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsSection")]
[Alias("TeamsSection")]
[OutputType(typeof(TeamsMessageSection))]
public sealed class CmdletNewTeamsSection : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? SectionInput { get; set; }

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? ActivityTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string? ActivitySubtitle { get; set; }

    [Parameter(Mandatory = false)]
    public string? ActivityImageLink { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Alert", "Cancel", "Disable", "Download", "Minus", "Check", "Add", "None")]
    public string ActivityImage { get; set; } = "None";

    [Parameter(Mandatory = false)]
    public FileInfo? ActivityImagePath { get; set; }

    [Parameter(Mandatory = false)]
    public string? ActivityText { get; set; }

    [Parameter(Mandatory = false)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsMessageFact[]? ActivityDetails { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsMessageButton[]? Buttons { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter StartGroup { get; set; }

    protected override void ProcessRecord() {
        var section = new TeamsMessageSection {
            Title = Title,
            ActivityTitle = ActivityTitle,
            ActivitySubtitle = ActivitySubtitle,
            ActivityText = ActivityText,
            Text = Text,
            StartGroup = StartGroup.IsPresent
        };

        if (ActivityImagePath is not null) {
            ValidateActivityImagePath(ActivityImagePath);
            section.ActivityImage = TeamsImageDataUtility.FromFile(ActivityImagePath.FullName);
        } else if (!string.Equals(ActivityImage, "None", StringComparison.OrdinalIgnoreCase)) {
            section.ActivityImage = ResolveBuiltInImage(ActivityImage);
        } else if (!string.IsNullOrWhiteSpace(ActivityImageLink)) {
            section.ActivityImage = ActivityImageLink;
        }

        if (ActivityDetails is not null) {
            foreach (var fact in ActivityDetails) {
                section.Facts.Add(fact);
            }
        }

        if (Buttons is not null) {
            foreach (var button in Buttons) {
                section.Buttons.Add(button);
            }
        }

        if (SectionInput is not null) {
            foreach (var item in SectionInput.Invoke()) {
                ApplySectionItem(section, item);
            }
        }

        WriteObject(section);
    }

    private void ApplySectionItem(TeamsMessageSection section, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsMessageButton button) {
            section.Buttons.Add(button);
            return;
        }

        if (value is TeamsMessageFact fact) {
            section.Facts.Add(fact);
            return;
        }

        if (value is TeamsMessageImage image) {
            AddMessageImage(section, image);
            return;
        }

        if (value is TeamsMessageSectionDirective directive) {
            ApplyDirective(section, directive);
            return;
        }

        if (value is IDictionary dictionary) {
            var markerType = GetDictionaryString(dictionary, "Type");
            switch (markerType) {
                case "button":
                    section.Buttons.Add(new TeamsMessageButton {
                        Name = GetDictionaryString(dictionary, "name") ?? GetDictionaryString(dictionary, "Name"),
                        Link = GetButtonLink(dictionary),
                        ButtonType = ParseButtonType(GetDictionaryString(dictionary, "@type"))
                    });
                    return;
                case "fact":
                    section.Facts.Add(new TeamsMessageFact {
                        Name = GetDictionaryString(dictionary, "name"),
                        Value = GetDictionaryString(dictionary, "value")
                    });
                    return;
                case "image":
                    var imageValue = GetDictionaryString(dictionary, "image");
                    if (!string.IsNullOrWhiteSpace(imageValue)) {
                        section.Images.Add(imageValue!);
                    }
                    return;
                case "HeroImageWorkaround":
                    var heroImage = GetDictionaryString(dictionary, "image");
                    if (!string.IsNullOrWhiteSpace(heroImage)) {
                        section.HeroImages.Add(heroImage!);
                    }
                    return;
                case "ActivityTitle":
                    section.ActivityTitle = GetDictionaryString(dictionary, "ActivityTitle");
                    return;
                case "ActivitySubtitle":
                    section.ActivitySubtitle = GetDictionaryString(dictionary, "ActivitySubtitle");
                    return;
                case "ActivityText":
                    section.ActivityText = GetDictionaryString(dictionary, "ActivityText");
                    return;
                case "ActivityImage":
                case "ActivityImageLink":
                    section.ActivityImage = GetDictionaryString(dictionary, "ActivityImageLink");
                    return;
            }
        }
    }

    private static void ApplyDirective(TeamsMessageSection section, TeamsMessageSectionDirective directive) {
        switch (directive.DirectiveType) {
            case TeamsMessageSectionDirectiveType.ActivityTitle:
                section.ActivityTitle = directive.Value;
                return;
            case TeamsMessageSectionDirectiveType.ActivitySubtitle:
                section.ActivitySubtitle = directive.Value;
                return;
            case TeamsMessageSectionDirectiveType.ActivityText:
                section.ActivityText = directive.Value;
                return;
            case TeamsMessageSectionDirectiveType.ActivityImage:
                section.ActivityImage = directive.Value;
                return;
        }
    }

    private static void AddMessageImage(TeamsMessageSection section, TeamsMessageImage image) {
        if (string.IsNullOrWhiteSpace(image.Image)) {
            return;
        }

        if (image.IsHeroImage) {
            section.HeroImages.Add(image.Image!);
            return;
        }

        section.Images.Add(image.Image!);
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }

    private static string? GetButtonLink(IDictionary dictionary) {
        if (dictionary.Contains("target")) {
            var targetValue = dictionary["target"];
            if (targetValue is IEnumerable enumerable && targetValue is not string) {
                foreach (var entry in enumerable) {
                    return entry?.ToString();
                }
            }
        }

        if (dictionary.Contains("Target")) {
            return dictionary["Target"]?.ToString();
        }

        if (dictionary.Contains("Targets")) {
            var targetsValue = dictionary["Targets"];
            if (targetsValue is IEnumerable targets && targetsValue is not string) {
                foreach (var entry in targets) {
                    if (entry is IDictionary targetDictionary && targetDictionary.Contains("uri")) {
                        return targetDictionary["uri"]?.ToString();
                    }
                }
            }
        }

        return null;
    }

    private static TeamsMessageButtonType ParseButtonType(string? payloadType) {
        return payloadType switch {
            "ActionCard" => TeamsMessageButtonType.TextInput,
            "HttpPOST" => TeamsMessageButtonType.HttpPost,
            "OpenURI" => TeamsMessageButtonType.OpenUri,
            _ => TeamsMessageButtonType.ViewAction
        };
    }

    private static void ValidateActivityImagePath(FileInfo path) {
        TeamsPowerShellImageSupport.ValidateImageFile(
            path,
            nameof(path),
            "ActivityImagePath is inaccessible or does not exist",
            "ActivityImagePath is not a file or file extension is not supported");
    }

    private static string ResolveBuiltInImage(string imageName) {
        return TeamsPowerShellImageSupport.ResolveBuiltInImage(imageName);
    }
}
