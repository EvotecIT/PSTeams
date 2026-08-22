---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordChannelTarget
## SYNOPSIS
Creates a Discord bot channel target.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordChannelTarget [-ChannelId] <string> [-GuildId <string>] [-DisplayName <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord bot channel target.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordChannelTarget -ChannelId 'Value'
```


## PARAMETERS

### -ChannelId
Discord channel identifier.

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

### -GuildId
Optional guild identifier retained in durable references.

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

- `MessageX.Discord.DiscordMessageTarget`

## RELATED LINKS

- None
