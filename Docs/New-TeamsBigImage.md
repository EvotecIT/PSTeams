---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsBigImage
## SYNOPSIS
Creates a hero-style markdown image entry for section text.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsBigImage [[-Link] <string>] [-AlternativeText <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a hero-style markdown image entry for section text.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsBigImage -AlternativeText 'Value'
```


## PARAMETERS

### -AlternativeText
Specifies a value for alternative text.

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

### -Link
Specifies a value for link.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Url, Uri
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

- `TeamsX.TeamsMessageImage`

## RELATED LINKS

- None
