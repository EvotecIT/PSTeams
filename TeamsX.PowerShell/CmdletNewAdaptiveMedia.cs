using System.Collections;
using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive media element backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveMedia")]
[OutputType(typeof(TeamsAdaptiveMedia))]
public sealed class CmdletNewAdaptiveMedia : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock Sources { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? PosterUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? AlternateText { get; set; }

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
        var media = new TeamsAdaptiveMedia {
            Poster = PosterUrl,
            AltText = AlternateText,
            Spacing = Spacing,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Id = Id,
            Separator = Separator.IsPresent ? true : null,
            IsVisible = Hidden.IsPresent ? false : null
        };

        foreach (var item in Sources.Invoke()) {
            ApplySource(media, item);
        }

        WriteObject(media);
    }

    private static void ApplySource(TeamsAdaptiveMedia media, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsAdaptiveMediaSource mediaSource) {
            media.Sources.Add(mediaSource);
            return;
        }

        if (value is IDictionary dictionary && TryCreateMediaSource(dictionary, out var converted)) {
            media.Sources.Add(converted);
        }
    }

    private static bool TryCreateMediaSource(IDictionary dictionary, out TeamsAdaptiveMediaSource mediaSource) {
        mediaSource = null!;

        var mimeType = GetDictionaryString(dictionary, "mimeType") ?? GetDictionaryString(dictionary, "MimeType");
        var url = GetDictionaryString(dictionary, "url") ?? GetDictionaryString(dictionary, "Url");
        if (string.IsNullOrWhiteSpace(mimeType) && string.IsNullOrWhiteSpace(url)) {
            return false;
        }

        mediaSource = new TeamsAdaptiveMediaSource {
            MimeType = mimeType ?? string.Empty,
            Url = url ?? string.Empty
        };
        return true;
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }
}
