---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsWebhookTarget
## SYNOPSIS
Creates a send-only Teams incoming webhook or Power Automate Workflow target.

## SYNTAX
### IncomingWebhook (Default)
```powershell
New-TeamsWebhookTarget [-Uri] <uri> [-DisplayName <string>] [<CommonParameters>]
```

### WorkflowWebhook
```powershell
New-TeamsWebhookTarget [-Uri] <uri> -Workflow [-DisplayName <string>] [-Destination <TeamsWorkflowDestinationKind>] [<CommonParameters>]
```

## DESCRIPTION
Workflow destination metadata documents where the configured flow delivers messages. It does not add reply, update, delete, or conversation capabilities.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsWebhookTarget -Uri $workflowUrl -Workflow -Destination Channel -DisplayName 'Release alerts'
```


### EXAMPLE 2
```powershell
New-TeamsWebhookTarget -Uri $incomingWebhookUrl -DisplayName 'Legacy alerts'
```


## PARAMETERS

### -Destination
Specifies a value for destination.

```yaml
Type: TeamsWorkflowDestinationKind
Parameter Sets: WorkflowWebhook
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
Parameter Sets: IncomingWebhook, WorkflowWebhook
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
Parameter Sets: IncomingWebhook, WorkflowWebhook
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Workflow
Specifies the workflow switch.

```yaml
Type: SwitchParameter
Parameter Sets: WorkflowWebhook
Aliases: None
Possible values:

Required: True
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

- `MessageX.Teams.TeamsMessageTarget`

## RELATED LINKS

- None
