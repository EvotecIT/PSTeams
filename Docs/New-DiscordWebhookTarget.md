---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordWebhookTarget
## SYNOPSIS
Creates a Discord incoming-webhook target, optionally within an existing thread.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordWebhookTarget [-Uri] <uri> [-ThreadId <string>] [-DisplayName <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord incoming-webhook target, optionally within an existing thread.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordWebhookTarget -DisplayName 'Name'
```


## PARAMETERS

### -DisplayName
Optional safe display label.

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

### -ThreadId
Optional existing thread identifier.

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
Secret Discord incoming-webhook URI.

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

- `MessageX.Discord.DiscordMessageTarget`

## RELATED LINKS

- None
