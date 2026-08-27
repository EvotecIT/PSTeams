---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordStringSelect
## SYNOPSIS
Creates a Discord string select menu.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordStringSelect [-CustomId] <string> [-Options] <DiscordSelectOption[]> [-Placeholder <string>] [-MinimumValues <int>] [-MaximumValues <int>] [-Disabled] [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord string select menu.

## EXAMPLES

### EXAMPLE 1
```powershell
$option = New-DiscordSelectOption -Label 'Production' -Value 'prod'; New-DiscordStringSelect -CustomId 'environment' -Options $option -Placeholder 'Choose an environment'
```


## PARAMETERS

### -CustomId
Application-defined identifier.

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

### -Disabled
Creates a disabled select menu.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaximumValues
Maximum number of values.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinimumValues
Minimum number of values.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Options
Selectable options.

```yaml
Type: DiscordSelectOption[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Placeholder
Placeholder shown before selection.

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

- `MessageX.Discord.DiscordStringSelect`

## RELATED LINKS

- None
