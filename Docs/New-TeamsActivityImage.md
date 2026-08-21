---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsActivityImage
## SYNOPSIS
Creates a typed activity-image directive for connector-card sections.

## SYNTAX
### Link (Default)
```powershell
New-TeamsActivityImage [-Link <string>] [<CommonParameters>]
```

### Image
```powershell
New-TeamsActivityImage [-Image <string>] [<CommonParameters>]
```

### Path
```powershell
New-TeamsActivityImage -Path <FileInfo> [<CommonParameters>]
```

## DESCRIPTION
Creates a typed activity-image directive for connector-card sections.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsActivityImage -Path 'C:\Path'
```


## PARAMETERS

### -Image
Specifies a value for image.

```yaml
Type: String
Parameter Sets: Image
Aliases: None
Possible values: Add, Alert, Cancel, Check, Disable, Download, Info, Minus, Question, Reload, None

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
Parameter Sets: Link
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Specifies a value for path.

```yaml
Type: FileInfo
Parameter Sets: Path
Aliases: None
Possible values:

Required: True
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

- `MessageX.Teams.TeamsMessageSectionDirective`

## RELATED LINKS

- None
