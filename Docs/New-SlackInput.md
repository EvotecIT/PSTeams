---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackInput
## SYNOPSIS
Creates a Slack modal input block.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackInput [-Label] <string> [-Element] <SlackBlockElement> [-Optional] [-Hint <string>] [-BlockId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack modal input block.

## EXAMPLES

### EXAMPLE 1
```powershell
$input = New-SlackPlainTextInput -ActionId 'reason' -Multiline -MaximumLength 500; New-SlackInput -Label 'Reason' -Element $input -BlockId 'reason-block'
```


## PARAMETERS

### -BlockId
Optional unique block identifier.

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

### -Element
Input element.

```yaml
Type: SlackBlockElement
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Hint
Optional plain-text hint.

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

### -Label
Plain-text field label.

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

### -Optional
Allows the user to omit this input.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackInputBlock`

## RELATED LINKS

- None
