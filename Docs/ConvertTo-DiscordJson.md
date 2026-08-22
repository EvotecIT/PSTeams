---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# ConvertTo-DiscordJson
## SYNOPSIS
Serializes a typed Discord message using the exact provider payload contract.

## SYNTAX
### __AllParameterSets
```powershell
ConvertTo-DiscordJson [-Message] <DiscordMessageRequest> [-Target] <DiscordMessageTarget> [<CommonParameters>]
```

## DESCRIPTION
Serializes a typed Discord message using the exact provider payload contract.

## EXAMPLES

### EXAMPLE 1
```powershell
ConvertTo-DiscordJson -Message 'Value'
```


## PARAMETERS

### -Message
Discord message to serialize.

```yaml
Type: DiscordMessageRequest
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Target
Discord target whose transport determines the payload envelope.

```yaml
Type: DiscordMessageTarget
Parameter Sets: __AllParameterSets
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

- `MessageX.Discord.DiscordMessageRequest`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None
