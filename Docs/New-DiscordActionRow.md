---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordActionRow
## SYNOPSIS
Creates a Discord action row.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordActionRow [-Components] <DiscordInteractiveComponent[]> [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord action row.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordActionRow -Components (New-DiscordButton -Label 'Approve' -CustomId 'approve')
```


## PARAMETERS

### -Components
Compatible buttons, one select menu, or one modal text input.

```yaml
Type: DiscordInteractiveComponent[]
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

- `MessageX.Discord.DiscordActionRow`

## RELATED LINKS

- None
