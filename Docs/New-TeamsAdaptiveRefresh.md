---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveRefresh
## SYNOPSIS
Creates an Adaptive Card refresh model for a bot-capable outbound transport. Current webhook targets reject it.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveRefresh [-Action] <TeamsAdaptiveExecuteAction> [-UserId <string[]>] [<CommonParameters>]
```

## DESCRIPTION
Creates an Adaptive Card refresh model for a bot-capable outbound transport. Current webhook targets reject it.

## EXAMPLES

### EXAMPLE 1
```powershell
$action = New-TeamsAdaptiveExecuteAction -Title 'Refresh' -Verb 'refresh'; New-TeamsAdaptiveRefresh -Action $action -UserId '29:example-user-id'
```


## PARAMETERS

### -Action
Refresh action.

```yaml
Type: TeamsAdaptiveExecuteAction
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserId
Optional Teams user identifiers that receive automatic refreshes.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsAdaptiveRefresh`

## RELATED LINKS

- None
