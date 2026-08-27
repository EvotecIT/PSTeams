---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveExecuteAction
## SYNOPSIS
Creates a Teams Universal Action model for a bot-capable outbound transport. Current webhook targets reject it.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveExecuteAction [-Title] <string> [-Verb] <string> [-Id <string>] [-Data <IDictionary>] [-AssociatedInputs <string>] [-Fallback <TeamsAdaptiveSubmitAction>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Teams Universal Action model for a bot-capable outbound transport. Current webhook targets reject it.

## EXAMPLES

### EXAMPLE 1
```powershell
$fallback = New-TeamsAdaptiveSubmitAction -Title 'Approve' -Data @{ action = 'approve' }; New-TeamsAdaptiveExecuteAction -Title 'Approve' -Verb 'approve' -Data @{ incident = 'INC-42' } -Fallback $fallback
```


## PARAMETERS

### -AssociatedInputs
Input association policy.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, None

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Data
JSON-compatible action data.

```yaml
Type: IDictionary
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Fallback
Optional Action.Submit fallback for older clients.

```yaml
Type: TeamsAdaptiveSubmitAction
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Optional action identifier.

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
Button label.

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

### -Verb
Application route verb.

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

- `MessageX.Teams.TeamsAdaptiveExecuteAction`

## RELATED LINKS

- None
