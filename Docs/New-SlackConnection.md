---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackConnection
## SYNOPSIS
Creates an authenticated Slack bot connection without exposing its token.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackConnection [-BotToken] <securestring> [-ApiBaseUri <uri>] [-WorkspaceId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates an authenticated Slack bot connection without exposing its token.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackConnection -ApiBaseUri 'Value'
```


## PARAMETERS

### -ApiBaseUri
Optional Slack or GovSlack API base URI.

```yaml
Type: Uri
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BotToken
Slack bot token stored as a secure string.

```yaml
Type: SecureString
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WorkspaceId
Optional non-secret workspace identifier used in delivery references.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackConnection`

## RELATED LINKS

- None
