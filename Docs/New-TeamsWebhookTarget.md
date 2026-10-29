---
external help file: MessageX-help.xml
Module Name: MessageX
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsWebhookTarget
## SYNOPSIS
Creates a send-only Teams Workflow webhook target.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsWebhookTarget [-Uri] <uri> [-DisplayName <string>] [-Destination <TeamsWorkflowDestinationKind>] [<CommonParameters>]
```

## DESCRIPTION
Creates a send-only Teams Workflow webhook target.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsWebhookTarget -Uri $workflowUrl -Destination Channel -DisplayName 'Release alerts'
```


## PARAMETERS

### -Destination
Specifies a value for destination.

```yaml
Type: TeamsWorkflowDestinationKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Unknown, Channel, GroupChat, Chat

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Specifies a value for display name.

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

### -Uri
Specifies a value for uri.

```yaml
Type: Uri
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

- `MessageX.Teams.TeamsMessageTarget`

## RELATED LINKS

- None
