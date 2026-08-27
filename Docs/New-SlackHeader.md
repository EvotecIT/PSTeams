---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackHeader
## SYNOPSIS
Creates a Slack Block Kit header.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackHeader [-Text] <string> [-BlockId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit header.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackHeader -Text 'Production incident' -BlockId 'incident-header'
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

### -Text
Plain-text header.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackHeaderBlock`

## RELATED LINKS

- None
