---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsAdaptiveMention
## SYNOPSIS
New-TeamsAdaptiveMention [-Text] <string> [-UserPrincipalName] <string> [[-Name] <string>] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsAdaptiveMention [-Text] <string> [-UserPrincipalName] <string> [[-Name] <string>] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsAdaptiveMention [-Text] <string> [-UserPrincipalName] <string> [[-Name] <string>] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsAdaptiveMention -Name 'Name'
```


## PARAMETERS

### -Name
Specifies a value for name.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Specifies a value for text.

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

### -UserPrincipalName
Specifies a value for user principal name.

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

- `TeamsX.TeamsAdaptiveMention`

## RELATED LINKS

- None
