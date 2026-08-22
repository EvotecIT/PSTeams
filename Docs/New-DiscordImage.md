---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordImage
## SYNOPSIS
Creates Discord embed image or thumbnail media.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordImage [-Url] <uri> [<CommonParameters>]
```

## DESCRIPTION
Creates Discord embed image or thumbnail media.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordImage -Url 'Value'
```


## PARAMETERS

### -Url
HTTPS or attachment media URI.

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

- `MessageX.Discord.DiscordEmbedMedia`

## RELATED LINKS

- None
