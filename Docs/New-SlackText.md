---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-SlackText
## SYNOPSIS
Creates typed Slack plain-text or mrkdwn content.

## SYNTAX
### Markdown (Default)
```powershell
New-SlackText [-Markdown] <string> [-Verbatim] [<CommonParameters>]
```

### PlainText
```powershell
New-SlackText [-PlainText] <string> [-Emoji] [<CommonParameters>]
```

## DESCRIPTION
Creates typed Slack plain-text or mrkdwn content.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SlackText -Emoji
```


## PARAMETERS

### -Emoji
Requests emoji conversion for plain text.

```yaml
Type: SwitchParameter
Parameter Sets: PlainText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Markdown
Slack mrkdwn content.

```yaml
Type: String
Parameter Sets: Markdown
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PlainText
Slack plain-text content.

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

### -Verbatim
Disables automatic link and mention conversion for mrkdwn.

```yaml
Type: SwitchParameter
Parameter Sets: Markdown
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

- `MessageX.Slack.SlackTextObject`

## RELATED LINKS

- None
