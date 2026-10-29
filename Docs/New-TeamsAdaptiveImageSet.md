---
external help file: MessageX-help.xml
Module Name: MessageX
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveImageSet
## SYNOPSIS
New-TeamsAdaptiveImageSet [-Images] <scriptblock> [-ImageSize <string>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveImageSet [-Images] <scriptblock> [-ImageSize <string>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveImageSet [-Images] <scriptblock> [-ImageSize <string>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveImageSet -Images { }
```


## PARAMETERS

### -Images
Specifies a value for images.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ImageSize
Specifies a value for image size.

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

- `MessageX.Teams.TeamsAdaptiveImageSet`

## RELATED LINKS

- None
