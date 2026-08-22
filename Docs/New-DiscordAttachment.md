---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordAttachment
## SYNOPSIS
Creates an in-memory Discord attachment from a file or byte array.

## SYNTAX
### Path (Default)
```powershell
New-DiscordAttachment [-Path] <string> [-Description <string>] [-ContentType <string>] [-Spoiler] [<CommonParameters>]
```

### Bytes
```powershell
New-DiscordAttachment [-Bytes] <byte[]> [-FileName] <string> [-Description <string>] [-ContentType <string>] [-Spoiler] [<CommonParameters>]
```

## DESCRIPTION
Creates an in-memory Discord attachment from a file or byte array.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordAttachment -Path 'C:\Path'
```


## PARAMETERS

### -Bytes
Attachment bytes.

```yaml
Type: Byte[]
Parameter Sets: Bytes
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ContentType
Optional MIME content type.

```yaml
Type: String
Parameter Sets: Path, Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Optional accessible attachment description.

```yaml
Type: String
Parameter Sets: Path, Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileName
File name used with the byte-array parameter set.

```yaml
Type: String
Parameter Sets: Bytes
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Local file path.

```yaml
Type: String
Parameter Sets: Path
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Spoiler
Marks the attachment as a spoiler.

```yaml
Type: SwitchParameter
Parameter Sets: Path, Bytes
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

- `MessageX.Discord.DiscordAttachment`

## RELATED LINKS

- None
