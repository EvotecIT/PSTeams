---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsCardImage
## SYNOPSIS
Creates an image entry for HeroCard or ThumbnailCard content.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsCardImage [[-Url] <string>] [-AlternateText <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates an image entry for HeroCard or ThumbnailCard content.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsCardImage -AlternateText 'Value'
```


## PARAMETERS

### -AlternateText
Specifies a value for alternate text.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: AltText, Alt
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
Specifies a value for url.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Link
Possible values:

Required: False
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

- `MessageX.Teams.TeamsCardImage`

## RELATED LINKS

- None
