---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-CardList
## SYNOPSIS
Creates or sends a Teams ListCard payload.

## SYNTAX
### __AllParameterSets
```powershell
New-CardList [-Content] <scriptblock> [-Title <string>] [-Uri <uri>] [-Proxy <uri>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or sends a Teams ListCard payload.

## EXAMPLES

### EXAMPLE 1
```powershell
New-CardList -Content { }
```


## PARAMETERS

### -Content
Specifies a value for content.

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

### -Proxy
Gets or sets the HTTP proxy used when the card is sent.

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

### -Title
Specifies a value for title.

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

### -Uri
Specifies a value for uri.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None
