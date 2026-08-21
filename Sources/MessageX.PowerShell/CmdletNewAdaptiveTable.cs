using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive table by projecting objects into column sets.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveTable")]
[OutputType(typeof(TeamsAdaptiveColumnSet))]
public sealed class CmdletNewAdaptiveTable : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public object[]? DataTable { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Auto", "Stretch")]
    public string Width { get; set; } = "Stretch";

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Dark", "Light", "Good", "Warning", "Attention")]
    public string HeaderColor { get; set; } = "Accent";

    [Alias("HeaderFontWeight")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Lighter", "Default", "Bolder")]
    public string HeaderWeight { get; set; } = "Bolder";

    [Alias("HeaderFontSize")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Small", "Default", "Medium", "Large", "ExtraLarge")]
    public string? HeaderSize { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter HeaderHighlight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter HeaderItalic { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter HeaderStrikeThrough { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Default", "Monospace")]
    public string? HeaderFontType { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? HeaderSpacing { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HeaderHorizontalAlignment { get; set; }

    [Alias("HeaderBlockElementHeight")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? HeaderHeight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter HeaderSubtle { get; set; }

    [Parameter(Mandatory = false)]
    public int HeaderMaximumLines { get; set; }

    [Alias("FontWeight")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Lighter", "Default", "Bolder")]
    public string? Weight { get; set; }

    [Alias("FontSize")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Small", "Default", "Medium", "Large", "ExtraLarge")]
    public string? Size { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Dark", "Light", "Good", "Warning", "Attention")]
    public string? Color { get; set; }

    [Parameter(Mandatory = false)]
    public bool Highlight { get; set; }

    [Parameter(Mandatory = false)]
    public bool Italic { get; set; }

    [Parameter(Mandatory = false)]
    public bool StrikeThrough { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Default", "Monospace")]
    public string[] FontType { get; set; } = Array.Empty<string>();

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Wrap { get; set; }

    [Alias("BlockElementHeight")]
    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Subtle { get; set; }

    [Parameter(Mandatory = false)]
    public int MaximumLines { get; set; }

    [Alias("HashTableAsCustomObject")]
    [Parameter(Mandatory = false)]
    public SwitchParameter DictionaryAsCustomObject { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter DisableHeaderColumnSeparators { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter DisableRowSeparators { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter DisableColumnSeparators { get; set; }

    protected override void ProcessRecord() {
        if (DataTable is not { Length: > 0 }) {
            return;
        }

        var rows = DataTable
            .Where(static row => row is not null)
            .ToArray();
        if (rows.Length == 0) {
            return;
        }

        var firstRow = rows[0];
        if (firstRow is IDictionary dictionary) {
            if (DictionaryAsCustomObject.IsPresent) {
                WriteDictionaryAsCustomObjectTable(rows.Cast<IDictionary>().ToArray(), dictionary);
                return;
            }

            WriteDictionaryAsNameValueTable(rows.Cast<IDictionary>().ToArray());
            return;
        }

        WriteObjectTable(rows);
    }

    private void WriteDictionaryAsCustomObjectTable(IDictionary[] rows, IDictionary firstRow) {
        var keys = firstRow.Keys.Cast<object?>()
            .Select(static key => key?.ToString() ?? string.Empty)
            .ToArray();

        WriteObject(CreateColumnSet(
            keys.Select(key => CreateColumn(
                key,
                separator: !DisableHeaderColumnSeparators.IsPresent,
                header: true))));

        foreach (var row in rows) {
            WriteObject(CreateColumnSet(
                keys.Select(key => CreateColumn(
                    row.Contains(key) ? row[key] : null,
                    separator: !DisableColumnSeparators.IsPresent)),
                separator: !DisableRowSeparators.IsPresent));
        }
    }

    private void WriteDictionaryAsNameValueTable(IDictionary[] rows) {
        WriteObject(CreateColumnSet(new[] {
            CreateColumn("Name", separator: !DisableHeaderColumnSeparators.IsPresent, header: true),
            CreateColumn("Value", separator: !DisableHeaderColumnSeparators.IsPresent, header: true)
        }));

        foreach (var row in rows) {
            foreach (var key in row.Keys.Cast<object?>()) {
                var keyText = key?.ToString();
                var value = key is null ? null : row[key];

                WriteObject(CreateColumnSet(new[] {
                    CreateColumn(keyText, separator: !DisableColumnSeparators.IsPresent),
                    CreateColumn(value, separator: !DisableColumnSeparators.IsPresent)
                }, separator: !DisableRowSeparators.IsPresent));
            }
        }
    }

    private void WriteObjectTable(object[] rows) {
        var propertyNames = PSObject.AsPSObject(rows[0]).Properties
            .Select(static property => property.Name)
            .ToArray();

        WriteObject(CreateColumnSet(
            propertyNames.Select(name => CreateColumn(
                name,
                separator: !DisableHeaderColumnSeparators.IsPresent,
                header: true))));

        foreach (var row in rows) {
            var rowObject = PSObject.AsPSObject(row);
            WriteObject(CreateColumnSet(
                propertyNames.Select(name => CreateColumn(
                    rowObject.Properties[name]?.Value,
                    separator: !DisableColumnSeparators.IsPresent)),
                separator: !DisableRowSeparators.IsPresent));
        }
    }

    private TeamsAdaptiveColumnSet CreateColumnSet(IEnumerable<TeamsAdaptiveColumn> columns, bool separator = false) {
        var columnSet = new TeamsAdaptiveColumnSet {
            Separator = separator ? true : null
        };

        foreach (var column in columns) {
            columnSet.Columns.Add(column);
        }

        return columnSet;
    }

    private TeamsAdaptiveColumn CreateColumn(object? value, bool separator, bool header = false) {
        var column = new TeamsAdaptiveColumn {
            Width = Width.ToLowerInvariant(),
            Separator = separator ? true : null
        };

        column.Items.Add(CreateTextBlock(value, header));
        return column;
    }

    private TeamsAdaptiveTextBlock CreateTextBlock(object? value, bool header) {
        return new TeamsAdaptiveTextBlock {
            Text = value?.ToString() ?? string.Empty,
            Weight = header ? HeaderWeight : Weight,
            Color = header ? HeaderColor : Color,
            Wrap = header ? null : (Wrap.IsPresent ? true : null),
            Size = header ? HeaderSize : Size,
            Highlight = header
                ? (HeaderHighlight.IsPresent ? true : null)
                : (Highlight ? true : null),
            Italic = header
                ? (HeaderItalic.IsPresent ? true : null)
                : (Italic ? true : null),
            StrikeThrough = header
                ? (HeaderStrikeThrough.IsPresent ? true : null)
                : (StrikeThrough ? true : null),
            FontType = header ? HeaderFontType : FontType.FirstOrDefault(),
            Spacing = header ? HeaderSpacing : Spacing,
            HorizontalAlignment = header ? HeaderHorizontalAlignment : HorizontalAlignment,
            Height = header ? HeaderHeight : Height,
            MaximumLines = header
                ? (HeaderMaximumLines > 0 ? HeaderMaximumLines : null)
                : (MaximumLines > 0 ? MaximumLines : null),
            Subtle = header
                ? (HeaderSubtle.IsPresent ? true : null)
                : (Subtle.IsPresent ? true : null)
        };
    }
}
