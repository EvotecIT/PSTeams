---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordFooter
## SYNOPSIS
Creates Discord embed footer metadata.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordFooter [-Text] <string> [-IconUrl <uri>] [<CommonParameters>]
```

## DESCRIPTION
Creates Discord embed footer metadata.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordFooter -IconUrl 'Value'
```


## PARAMETERS

### -IconUrl
Optional footer icon.

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

### -Text
Footer text.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Discord.DiscordEmbedFooter`

## RELATED LINKS

- None
