---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveMention

## SYNOPSIS
Allows Adaptive Card to mention specific person in the notification.

## SYNTAX

```
New-AdaptiveMention [-Text] <String> [-UserPrincipalName] <String> [[-Name] <String>] [<CommonParameters>]
```

## DESCRIPTION
Allows Adaptive Card to mention specific person in the notification.
Currently Adaptive Card in incoming webhook notifications allows you to define a mention property, but it doesn't notify the user on notification.
It's supposed to be enabled in future by Microsoft.

## EXAMPLES

### EXAMPLE 1
```
przemyslaw.klys</at>' -UserPrincipalName 'przemyslaw.klys@evotec.test'
```

### EXAMPLE 2
```
New-AdaptiveMention -Text 'przemyslaw.klys' -UserPrincipalName 'przemyslaw.klys@evotec.test' -Name 'Przemysław Kłys'
```

### EXAMPLE 3
```
New-AdaptiveCard -Uri $URI -VerticalContentAlignment center {
```

New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
    New-AdaptiveColumnSet {
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Dark
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Light
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Good
        }
        New-AdaptiveColumn {
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 \<at\>Name\</at\>' -Color Warning
            New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 \<at\>Zenon Jaskuła\</at\>' -Color Warning
        }
    }
    New-AdaptiveMention -Text 'Zenon Jaskuła' -UserPrincipalName 'przemyslaw.klys@evotec.test'
    New-AdaptiveMention -Text 'Name' -UserPrincipalName 'przemyslaw.klys@evotec.test'
} -Verbose

## PARAMETERS

### -Text
Provide the text to be mentioned by using \<at\>person\</at\> tag.
You can provide tags or skip them.
Should work either way.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserPrincipalName
Provide the user principal name of the person to be mentioned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Provide the name of the person to be mentioned.
This is optional.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
More information here: https://github.com/EvotecIT/PSTeams/issues/17

## RELATED LINKS
