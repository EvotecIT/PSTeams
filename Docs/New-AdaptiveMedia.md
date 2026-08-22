---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveMedia
## SYNOPSIS
Creates a legacy-named adaptive media element backed by the TeamsX model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveMedia [-Sources] <scriptblock> [-PosterUrl <string>] [-AlternateText <string>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive media element backed by the TeamsX model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveMedia -AlternateText 'Value'
```


## PARAMETERS

### -AlternateText
Specifies a value for alternate text.

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

### -PosterUrl
Specifies a value for poster url.

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

### -Sources
Specifies a value for sources.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `TeamsX.TeamsAdaptiveMedia`

## RELATED LINKS

- None
