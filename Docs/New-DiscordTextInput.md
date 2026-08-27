---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordTextInput
## SYNOPSIS
Creates a Discord modal text input.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordTextInput [-CustomId] <string> [-Label] <string> [-Style <DiscordTextInputStyle>] [-MinimumLength <Int32>] [-MaximumLength <Int32>] [-Optional] [-Value <string>] [-Placeholder <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord modal text input.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordTextInput -CustomId 'reason' -Label 'Reason' -Style Paragraph -MaximumLength 500 -Placeholder 'Explain the change'
```


## PARAMETERS

### -CustomId
Application-defined identifier.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
User-visible label.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaximumLength
Maximum accepted length.

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

### -MinimumLength
Minimum accepted length.

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

### -Optional
Allows an empty value.

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

### -Placeholder
Placeholder for an empty input.

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

### -Style
Input layout style.

```yaml
Type: DiscordTextInputStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Short, Paragraph

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Prepopulated value.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Discord.DiscordTextInput`

## RELATED LINKS

- None
