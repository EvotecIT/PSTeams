---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsButton
## SYNOPSIS
Creates a connector-card button/action.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsButton [-Name] <string> [-Link] <string> [-Type <TeamsMessageButtonType>] [<CommonParameters>]
```

## DESCRIPTION
Creates a connector-card button/action.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsButton -Name 'Open build' -Link 'https://ci.example.test/build/42' -Type OpenUri
```


## PARAMETERS

### -Link
Target URL or action value used when the button is selected.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: TargetUri, Uri, Url
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Text displayed on the button.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: ButtonName
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Type
Connector-card action type. The default is ViewAction.

```yaml
Type: TeamsMessageButtonType
Parameter Sets: __AllParameterSets
Aliases: ButtonType
Possible values: ViewAction, TextInput, DateInput, HttpPost, OpenUri

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

- `MessageX.Teams.TeamsMessageButton`

## RELATED LINKS

- None
