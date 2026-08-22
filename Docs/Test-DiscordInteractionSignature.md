---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Test-DiscordInteractionSignature
## SYNOPSIS
Verifies a Discord interaction signature and bounds its age. The hosting service must separately
reject duplicate signatures because an age window alone does not prevent replay.

## SYNTAX
### __AllParameterSets
```powershell
Test-DiscordInteractionSignature [-PublicKey] <string> [-Signature] <string> [-Timestamp] <string> [-Body] <byte[]> [-MaximumAgeSeconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Verifies a Discord interaction signature and bounds its age. The hosting service must separately
reject duplicate signatures because an age window alone does not prevent replay.

## EXAMPLES

### EXAMPLE 1
```powershell
Test-DiscordInteractionSignature -Body @('Value')
```


## PARAMETERS

### -Body
Exact raw request body bytes.

```yaml
Type: Byte[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaximumAgeSeconds
Maximum accepted clock skew and request age in seconds.

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

### -PublicKey
Discord application public key encoded as hexadecimal.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Signature
Ed25519 request signature from the X-Signature-Ed25519 header.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Timestamp
Unix timestamp text from the X-Signature-Timestamp header.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `System.Boolean`

## RELATED LINKS

- None
