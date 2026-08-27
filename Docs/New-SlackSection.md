---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackSection
## SYNOPSIS
Creates a Slack Block Kit section.

## SYNTAX
### Markdown (Default)
```powershell
New-SlackSection [[-Markdown] <string>] [-Fields <SlackTextObject[]>] [-BlockId <string>] [-Expand] [-Accessory <SlackBlockElement>] [<CommonParameters>]
```

### PlainText
```powershell
New-SlackSection [-PlainText] <string> [-Fields <SlackTextObject[]>] [-BlockId <string>] [-Expand] [-Accessory <SlackBlockElement>] [<CommonParameters>]
```

### Typed
```powershell
New-SlackSection [-TextObject] <SlackTextObject> [-Fields <SlackTextObject[]>] [-BlockId <string>] [-Expand] [-Accessory <SlackBlockElement>] [<CommonParameters>]
```

## DESCRIPTION
Creates a Slack Block Kit section.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackSection -Accessory 'Value'
```


## PARAMETERS

### -Accessory
Optional interactive accessory such as a button.

```yaml
Type: SlackBlockElement
Parameter Sets: Markdown, PlainText, Typed
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BlockId
Optional unique Slack block identifier.

```yaml
Type: String
Parameter Sets: Markdown, PlainText, Typed
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Expand
Requests that Slack initially expand long section text.

```yaml
Type: SwitchParameter
Parameter Sets: Markdown, PlainText, Typed
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Fields
Optional compact section fields.

```yaml
Type: SlackTextObject[]
Parameter Sets: Markdown, PlainText, Typed
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Markdown
Markdown section text.

```yaml
Type: String
Parameter Sets: Markdown
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PlainText
Plain section text.

```yaml
Type: String
Parameter Sets: PlainText
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextObject
Typed Slack text object.

```yaml
Type: SlackTextObject
Parameter Sets: Typed
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `MessageX.Slack.SlackSectionBlock`

## RELATED LINKS

- None
