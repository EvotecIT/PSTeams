---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackPlainTextInput
## SYNOPSIS
Creates a Slack modal plain-text input element.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackPlainTextInput [-ActionId] <string> [-InitialValue <string>] [-Multiline] [-MinimumLength <Int32>] [-MaximumLength <Int32>] [-Placeholder <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack modal plain-text input element.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackPlainTextInput -ActionId 'reason' -Multiline -MaximumLength 500 -Placeholder 'Explain the change'
```


## PARAMETERS

### -ActionId
Application-defined action identifier.

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

### -InitialValue
Initial value.

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

### -Multiline
Allows multiple input lines.

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
Optional placeholder.

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

- `MessageX.Slack.SlackPlainTextInputElement`

## RELATED LINKS

- None
