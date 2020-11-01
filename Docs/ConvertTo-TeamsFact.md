---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# ConvertTo-TeamsFact

## SYNOPSIS
Convert a PSCustomObject or a Hashtable to Teams facts.

## SYNTAX

```
ConvertTo-TeamsFact [-InputObject] <Object> [<CommonParameters>]
```

## DESCRIPTION
Teams facts are name-value pairs.
This function helps convert a PSObject or a Hashtable to Teams facts (only one level deep).

## EXAMPLES

### EXAMPLE 1
```
Get-ChildItem | Select-Object -First 1 | ConvertTo-TeamsFact
```

### EXAMPLE 2
```
@{ Product = 'Microsoft Teams'; Developer = 'Microsoft Corporation'; ReleaseYear = '2018' } | ConvertTo-TeamsFact
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
Ram Iyer (https://ramiyer.me)

## RELATED LINKS
