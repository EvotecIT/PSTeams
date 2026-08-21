using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive image set backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveImageSet")]
[OutputType(typeof(TeamsAdaptiveImageSet))]
public sealed class CmdletNewAdaptiveImageSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Images { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Small", "Medium", "Large")]
    public string? Size { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    protected override void ProcessRecord() {
        if (Images is null) {
            return;
        }

        var imageSet = new TeamsAdaptiveImageSet {
            Id = Id,
            ImageSize = Size,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null
        };

        foreach (var item in Images.Invoke()) {
            ApplyImage(imageSet, item);
        }

        if (imageSet.Images.Count > 0) {
            WriteObject(imageSet);
        }
    }

    private static void ApplyImage(TeamsAdaptiveImageSet imageSet, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsAdaptiveImage image) {
            imageSet.Images.Add(image);
            return;
        }

        if (value is IDictionary dictionary && TryCreateImage(dictionary, out var converted)) {
            imageSet.Images.Add(converted);
        }
    }

    private static bool TryCreateImage(IDictionary dictionary, out TeamsAdaptiveImage image) {
        image = null!;
        var resolvedUrl = GetDictionaryString(dictionary, "url") ?? GetDictionaryString(dictionary, "Url");
        if (string.IsNullOrWhiteSpace(resolvedUrl)) {
            return false;
        }

        image = new TeamsAdaptiveImage {
            Id = GetDictionaryString(dictionary, "id") ?? GetDictionaryString(dictionary, "Id"),
            Url = resolvedUrl!,
            AltText = GetDictionaryString(dictionary, "alt") ?? GetDictionaryString(dictionary, "altText") ?? GetDictionaryString(dictionary, "AltText"),
            Size = GetDictionaryString(dictionary, "size") ?? GetDictionaryString(dictionary, "Size"),
            Style = GetDictionaryString(dictionary, "style") ?? GetDictionaryString(dictionary, "Style"),
            HorizontalAlignment = GetDictionaryString(dictionary, "horizontalAlignment") ?? GetDictionaryString(dictionary, "HorizontalAlignment"),
            Height = GetDictionaryString(dictionary, "height") ?? GetDictionaryString(dictionary, "Height"),
            Width = GetDictionaryString(dictionary, "width") ?? GetDictionaryString(dictionary, "Width"),
            Spacing = GetDictionaryString(dictionary, "spacing") ?? GetDictionaryString(dictionary, "Spacing"),
            BackgroundColor = GetDictionaryString(dictionary, "backgroundColor") ?? GetDictionaryString(dictionary, "BackgroundColor"),
            Separator = GetDictionaryBoolean(dictionary, "separator") ?? GetDictionaryBoolean(dictionary, "Separator"),
            IsVisible = GetDictionaryBoolean(dictionary, "isVisible") ?? GetDictionaryBoolean(dictionary, "IsVisible")
        };
        return true;
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }

    private static bool? GetDictionaryBoolean(IDictionary dictionary, string key) {
        var value = GetDictionaryString(dictionary, key);
        return bool.TryParse(value, out var parsed) ? parsed : null;
    }
}
