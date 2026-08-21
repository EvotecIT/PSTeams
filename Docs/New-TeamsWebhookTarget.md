---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsWebhookTarget
## SYNOPSIS
New-TeamsWebhookTarget [-Uri] <uri> [-DisplayName <string>] [-Workflow] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsWebhookTarget [-Uri] <uri> [-DisplayName <string>] [-Workflow] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsWebhookTarget [-Uri] <uri> [-DisplayName <string>] [-Workflow] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsWebhookTarget -DisplayName 'Name'
```


## PARAMETERS

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

### -Workflow
Specifies the workflow switch.

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

- `MessageX.Teams.TeamsMessageTarget`

## RELATED LINKS

- None
