---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# ConvertTo-TeamsSection

## SYNOPSIS
Convert an array of PSCustomObject or a Hashtable to separate Teams sections.

## SYNTAX

```
ConvertTo-TeamsSection [-InputObject] <Object> [[-SectionTitleProperty] <String>] [<CommonParameters>]
```

## DESCRIPTION
Teams sections are chunks of information that appear within a Teams message.
This function helps convert an array of PSObject or an array of Hashtables to Teams sections (only one level deep).

## EXAMPLES

### EXAMPLE 1
```
Get-ChildItem -Directory | ConvertTo-TeamsSection -SectionTitleProperty Name
```

## PARAMETERS

### -InputObject
The Hashtable or PSObject that is output by another cmdlet.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SectionTitleProperty
The property to use for title

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
Ram Iyer (https://ramiyer.me)

## RELATED LINKS
