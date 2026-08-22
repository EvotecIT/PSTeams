---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordAllowedMentions
## SYNOPSIS
Creates an explicit Discord mention policy. The default policy notifies nobody.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordAllowedMentions [-ParseUsers] [-ParseRoles] [-ParseEveryone] [-UserId <string[]>] [-RoleId <string[]>] [-RepliedUser] [<CommonParameters>]
```

## DESCRIPTION
Creates an explicit Discord mention policy. The default policy notifies nobody.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordAllowedMentions -ParseEveryone
```


## PARAMETERS

### -ParseEveryone
Parses everyone and here mention syntax.

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

### -ParseRoles
Parses role mention syntax in message content.

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

### -ParseUsers
Parses user mention syntax in message content.

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

### -RepliedUser
Mentions the author of a replied-to message.

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

### -RoleId
Explicit role identifiers that may receive mentions.

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

### -UserId
Explicit user identifiers that may receive mentions.

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

- `MessageX.Discord.DiscordAllowedMentions`

## RELATED LINKS

- None
