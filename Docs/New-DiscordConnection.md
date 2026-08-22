---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordConnection
## SYNOPSIS
Creates an authenticated Discord bot connection without exposing its token.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordConnection [-BotToken] <securestring> [-ApplicationId <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates an authenticated Discord bot connection without exposing its token.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordConnection -ApplicationId 'Value'
```


## PARAMETERS

### -ApplicationId
Optional non-secret Discord application identifier.

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

### -BotToken
Discord bot token stored as a secure string.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Discord.DiscordConnection`

## RELATED LINKS

- None
