---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsMessage
## SYNOPSIS
New-TeamsMessage [-Title <string>] [-Text <string>] [-Summary <string>] [-AdaptiveCard <TeamsAdaptiveCard>] [-Sections <TeamsMessageSection[]>] [-ThemeColor <string>] [-HideOriginalBody] [-UseConnectorCardFormat] [<CommonParameters>]

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsMessage [-Title <string>] [-Text <string>] [-Summary <string>] [-AdaptiveCard <TeamsAdaptiveCard>] [-Sections <TeamsMessageSection[]>] [-ThemeColor <string>] [-HideOriginalBody] [-UseConnectorCardFormat] [<CommonParameters>]
```

## DESCRIPTION
New-TeamsMessage [-Title <string>] [-Text <string>] [-Summary <string>] [-AdaptiveCard <TeamsAdaptiveCard>] [-Sections <TeamsMessageSection[]>] [-ThemeColor <string>] [-HideOriginalBody] [-UseConnectorCardFormat] [<CommonParameters>]

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsMessage -AdaptiveCard 'Value'
```


## PARAMETERS

### -AdaptiveCard
Specifies a value for adaptive card.

```yaml
Type: TeamsAdaptiveCard
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HideOriginalBody
Specifies the hide original body switch.

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

### -Sections
Specifies one or more values for sections.

```yaml
Type: TeamsMessageSection[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Summary
Specifies a value for summary.

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

### -Text
Specifies a value for text.

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

### -ThemeColor
Specifies a value for theme color.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Color
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

### -UseConnectorCardFormat
Specifies the use connector card format switch.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsMessageRequest`

## RELATED LINKS

- None
