---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackModal
## SYNOPSIS
Creates a typed Slack modal view.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackModal [-CallbackId] <string> [-Title] <string> [-Blocks] <SlackBlock[]> [-Submit <string>] [-Close <string>] [-NotifyOnClose] [<CommonParameters>]
```

## DESCRIPTION
Creates a typed Slack modal view.

## EXAMPLES

### EXAMPLE 1
```powershell
$input = New-SlackPlainTextInput -ActionId 'reason' -Multiline; $block = New-SlackInput -Label 'Reason' -Element $input; New-SlackModal -CallbackId 'approval' -Title 'Approval' -Blocks $block -Submit 'Submit' -Close 'Cancel'
```


## PARAMETERS

### -Blocks
Modal Block Kit blocks.

```yaml
Type: SlackBlock[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CallbackId
Application-defined callback identifier.

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

### -Close
Optional close label.

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

### -NotifyOnClose
Requests a view_closed interaction.

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

### -Submit
Optional submit label.

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

### -Title
Plain-text modal title.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackModalView`

## RELATED LINKS

- None
