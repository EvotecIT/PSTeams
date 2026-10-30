---
external help file: MessageX-help.xml
Module Name: MessageX
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# ConvertTo-TeamsJson
## SYNOPSIS
Renders a Teams Workflow message as JSON without sending it.

## SYNTAX
### __AllParameterSets
```powershell
ConvertTo-TeamsJson [-InputObject] <TeamsMessageRequest> [<CommonParameters>]
```

## DESCRIPTION
Renders a Teams Workflow message as JSON without sending it.

## EXAMPLES

### EXAMPLE 1
```powershell
ConvertTo-TeamsJson -InputObject 'Value'
```


## PARAMETERS

### -InputObject
Specifies a value for input object.

```yaml
Type: TeamsMessageRequest
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `MessageX.Teams.TeamsMessageRequest`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None
