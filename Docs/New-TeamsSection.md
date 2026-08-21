---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# New-TeamsSection
## SYNOPSIS
Creates a connector-card section.

## SYNTAX
### __AllParameterSets
```powershell
New-TeamsSection [[-SectionInput] <scriptblock>] [-Title <string>] [-ActivityTitle <string>] [-ActivitySubtitle <string>] [-ActivityImageLink <string>] [-ActivityImage <string>] [-ActivityImagePath <FileInfo>] [-ActivityText <string>] [-Text <string>] [-ActivityDetails <TeamsMessageFact[]>] [-Buttons <TeamsMessageButton[]>] [-StartGroup] [<CommonParameters>]
```

## DESCRIPTION
Creates a connector-card section.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TeamsSection -Title 'Build 42' -ActivityText 'Deployment failed' -ActivityDetails (New-TeamsFact -Name 'Status' -Value 'Failed') -Buttons (New-TeamsButton -Name 'Open build' -Link 'https://ci.example.test/build/42')
```


## PARAMETERS

### -ActivityDetails
Fact rows displayed in the section.

```yaml
Type: TeamsMessageFact[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ActivityImage
Name of a built-in PSTeams activity image.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Alert, Cancel, Disable, Download, Minus, Check, Add, None

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ActivityImageLink
HTTPS URL for the activity image.

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

### -ActivityImagePath
Local image file to embed as the activity image.

```yaml
Type: FileInfo
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ActivitySubtitle
Activity subheading.

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

### -ActivityText
Text displayed beside the activity image.

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

### -ActivityTitle
Activity heading.

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

### -Buttons
Actions displayed in the section.

```yaml
Type: TeamsMessageButton[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SectionInput
Optional composition script block that emits facts, buttons, images, or section directives.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StartGroup
Starts a visually separated group before this section.

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

### -Text
Main section text.

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

### -Title
Section heading.

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

- `TeamsX.TeamsMessageSection`

## RELATED LINKS

- None
