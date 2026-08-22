---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveCard
## SYNOPSIS
New-TeamsAdaptiveCard [-Body <TeamsAdaptiveCardElement[]>] [-Actions <TeamsAdaptiveAction[]>] [-Mentions <TeamsAdaptiveMention[]>] [-Version <string>] [-FallbackText <string>] [-MinimumHeight <int>] [-Speak <string>] [-Language <string>] [-VerticalContentAlignment <string>] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-FullWidth] [-AllowImageExpand] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveCard [-Body <TeamsAdaptiveCardElement[]>] [-Actions <TeamsAdaptiveAction[]>] [-Mentions <TeamsAdaptiveMention[]>] [-Version <string>] [-FallbackText <string>] [-MinimumHeight <int>] [-Speak <string>] [-Language <string>] [-VerticalContentAlignment <string>] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-FullWidth] [-AllowImageExpand] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveCard [-Body <TeamsAdaptiveCardElement[]>] [-Actions <TeamsAdaptiveAction[]>] [-Mentions <TeamsAdaptiveMention[]>] [-Version <string>] [-FallbackText <string>] [-MinimumHeight <int>] [-Speak <string>] [-Language <string>] [-VerticalContentAlignment <string>] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-FullWidth] [-AllowImageExpand] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveCard -Actions @('Value')
```


## PARAMETERS

### -Actions
Specifies one or more values for actions.

```yaml
Type: TeamsAdaptiveAction[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AllowImageExpand
Specifies the allow image expand switch.

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

### -BackgroundFillMode
Specifies a value for background fill mode.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Cover, RepeatHorizontally, RepeatVertically, Repeat

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundHorizontalAlignment
Specifies a value for background horizontal alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: left, center, right

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundUrl
Specifies a value for background url.

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

### -BackgroundVerticalAlignment
Specifies a value for background vertical alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: top, center, bottom

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Body
Specifies one or more values for body.

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

### -FallbackText
Specifies a value for fallback text.

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

### -FullWidth
Specifies the full width switch.

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

### -Language
Specifies a value for language.

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

### -Mentions
Specifies one or more values for mentions.

```yaml
Type: TeamsAdaptiveMention[]
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

### -Speak
Specifies a value for speak.

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

### -Version
Specifies a value for version.

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

### -VerticalContentAlignment
Specifies a value for vertical content alignment.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: top, center, bottom

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

- `TeamsX.TeamsAdaptiveCard`

## RELATED LINKS

- None
