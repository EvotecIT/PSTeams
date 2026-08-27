---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordButton
## SYNOPSIS
Creates a Discord interactive or link button.

## SYNTAX
### Interactive (Default)
```powershell
New-DiscordButton [-Label] <string> [-CustomId] <string> [-Style <DiscordButtonStyle>] [-Disabled] [<CommonParameters>]
```

### Link
```powershell
New-DiscordButton [-Label] <string> [-Url] <uri> [-Disabled] [<CommonParameters>]
```

## DESCRIPTION
Creates a Discord interactive or link button.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordButton -Label 'Approve' -CustomId 'approve' -Style Success
```


## PARAMETERS

### -CustomId
Application-defined identifier.

```yaml
Type: String
Parameter Sets: Interactive
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Disabled
Creates a disabled button.

```yaml
Type: SwitchParameter
Parameter Sets: Interactive, Link
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
User-visible label.

```yaml
Type: String
Parameter Sets: Interactive, Link
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style
Interactive button style.

```yaml
Type: DiscordButtonStyle
Parameter Sets: Interactive
Aliases: None
Possible values: Primary, Secondary, Success, Danger, Link

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
External HTTPS URL for a link button.

```yaml
Type: Uri
Parameter Sets: Link
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

- `MessageX.Discord.DiscordButton`

## RELATED LINKS

- None
