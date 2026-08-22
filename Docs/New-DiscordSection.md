---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-DiscordSection
## SYNOPSIS
Creates a rich Discord embed.

## SYNTAX
### __AllParameterSets
```powershell
New-DiscordSection [[-Title] <string>] [[-Description] <string>] [-Url <uri>] [-Color <Int32>] [-Timestamp <DateTimeOffset>] [-Author <DiscordEmbedAuthor>] [-Footer <DiscordEmbedFooter>] [-Image <DiscordEmbedMedia>] [-Thumbnail <DiscordEmbedMedia>] [-Fields <DiscordEmbedField[]>] [<CommonParameters>]
```

## DESCRIPTION
Creates a rich Discord embed.

## EXAMPLES

### EXAMPLE 1
```powershell
New-DiscordSection -Author 'Value'
```


## PARAMETERS

### -Author
Optional author metadata.

```yaml
Type: DiscordEmbedAuthor
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Optional 24-bit RGB color.

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

### -Description
Optional embed description.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Fields
Optional embed fields.

```yaml
Type: DiscordEmbedField[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Footer
Optional footer metadata.

```yaml
Type: DiscordEmbedFooter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Image
Optional large image.

```yaml
Type: DiscordEmbedMedia
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Thumbnail
Optional compact thumbnail.

```yaml
Type: DiscordEmbedMedia
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Timestamp
Optional timestamp.

```yaml
Type: DateTimeOffset
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
Optional embed title.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
Optional link applied to the title.

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

- `MessageX.Discord.DiscordEmbed`

## RELATED LINKS

- None
