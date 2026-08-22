---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveImage
## SYNOPSIS
Creates a legacy-named adaptive image backed by the TeamsX model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveImage [[-Url] <string>] [-Style <string>] [-AlternateText <string>] [-Size <string>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-HeightInPixels <int>] [-WidthInPixels <int>] [-Id <string>] [-Hidden] [-BackgroundColor <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive image backed by the TeamsX model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveImage -AlternateText 'Value'
```


## PARAMETERS

### -AlternateText
Specifies a value for alternate text.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Alt, AltText
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundColor
Specifies a value for background color.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Specifies a value for height.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Stretch, Automatic

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeightInPixels
Specifies a value for height in pixels.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Hidden
Specifies the hidden switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HorizontalAlignment
Specifies a value for horizontal alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Left, Center, Right

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Specifies a value for id.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectAction
Specifies a value for select action.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Action.Submit, Action.OpenUrl, Action.ToggleVisibility

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionId
Specifies a value for select action id.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionTargetElement
Specifies one or more values for select action target element.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionTitle
Specifies a value for select action title.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionUrl
Specifies a value for select action url.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Separator
Specifies the separator switch.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Specifies a value for size.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, Stretch, Small, Medium, Large

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Spacing
Specifies a value for spacing.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Small, Default, Medium, Large, ExtraLarge, Padding

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style
Specifies a value for style.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: person, default

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
Specifies a value for url.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Link
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WidthInPixels
Specifies a value for width in pixels.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `TeamsX.TeamsAdaptiveImage`

## RELATED LINKS

- None
