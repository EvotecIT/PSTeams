---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackButton
## SYNOPSIS
Creates a Slack Block Kit button.

## SYNTAX
### __AllParameterSets
```powershell
New-SlackButton [-Text] <string> [-ActionId] <string> [-Value <string>] [-Url <uri>] [-Style <SlackButtonStyle>] [-AccessibilityLabel <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit button.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackButton -Text 'Approve' -ActionId 'approve' -Value 'release-42' -Style Primary
```


## PARAMETERS

### -AccessibilityLabel
Optional accessibility label.

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

### -ActionId
Application-defined action identifier.

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

### -Style
Visual button style.

```yaml
Type: SlackButtonStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Default, Primary, Danger

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
User-visible plain-text label.

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

### -Url
Optional external HTTPS URL.

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

### -Value
Optional application-defined interaction value.

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

- `MessageX.Slack.SlackButtonElement`

## RELATED LINKS

- None
