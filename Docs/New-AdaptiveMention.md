---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-AdaptiveMention
## SYNOPSIS
Creates a legacy-named adaptive mention backed by the MessageX.Teams model.

## SYNTAX
### __AllParameterSets
```powershell
New-AdaptiveMention [-Text] <string> [-UserPrincipalName] <string> [[-Name] <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a legacy-named adaptive mention backed by the MessageX.Teams model.

## EXAMPLES

### EXAMPLE 1
```powershell
New-AdaptiveMention -Name 'Name'
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

- `MessageX.Teams.TeamsAdaptiveMention`

## RELATED LINKS

- None
