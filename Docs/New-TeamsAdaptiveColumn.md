---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveColumn
## SYNOPSIS
New-TeamsAdaptiveColumn [-Width <string>] [-WidthInWeight <int>] [-WidthInPixels <int>] [-Height <string>] [-MinimumHeight <int>] [-HorizontalAlignment <string>] [-VerticalContentAlignment <string>] [-Spacing <string>] [-Style <string>] [-Hidden] [-Separator] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-Items <TeamsAdaptiveCardElement[]>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveColumn [-Width <string>] [-WidthInWeight <int>] [-WidthInPixels <int>] [-Height <string>] [-MinimumHeight <int>] [-HorizontalAlignment <string>] [-VerticalContentAlignment <string>] [-Spacing <string>] [-Style <string>] [-Hidden] [-Separator] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-Items <TeamsAdaptiveCardElement[]>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveColumn [-Width <string>] [-WidthInWeight <int>] [-WidthInPixels <int>] [-Height <string>] [-MinimumHeight <int>] [-HorizontalAlignment <string>] [-VerticalContentAlignment <string>] [-Spacing <string>] [-Style <string>] [-Hidden] [-Separator] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-Items <TeamsAdaptiveCardElement[]>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveColumn -Height 'Value'
```


## PARAMETERS

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

### -Items
Specifies one or more values for items.

```yaml
Type: TeamsAdaptiveCardElement[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinimumHeight
Specifies a value for minimum height.

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
Possible values: Accent, Default, Emphasis, Good, Warning, Attention

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerticalContentAlignment
Specifies a value for vertical content alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Top, Center, Bottom

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Specifies a value for width.

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

### -WidthInWeight
Specifies a value for width in weight.

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

- `TeamsX.TeamsAdaptiveColumn`

## RELATED LINKS

- None
