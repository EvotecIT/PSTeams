---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordModal
## SYNOPSIS
Creates a Discord modal for an immediate interaction response.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordModal [-CustomId] <string> [-Title] <string> [-Components] <DiscordActionRow[]> [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord modal for an immediate interaction response.

## EXAMPLES

### EXAMPLE 1
```powershell
$input = New-DiscordTextInput -CustomId 'reason' -Label 'Reason'; $row = New-DiscordActionRow -Components $input; New-DiscordModal -CustomId 'approval' -Title 'Approval' -Components $row
```


## PARAMETERS

### -Components
Action rows containing one text input each.

```yaml
Type: DiscordActionRow[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CustomId
Application-defined modal identifier.

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

### -Title
User-visible modal title.

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

- `MessageX.Discord.DiscordModalRequest`

## RELATED LINKS

- None
