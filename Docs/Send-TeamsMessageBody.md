---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Send-TeamsMessageBody
## SYNOPSIS
Sends a raw Teams message payload body to an incoming webhook.

## SYNTAX
### __AllParameterSets
```powershell
Send-TeamsMessageBody [-Uri] <uri> [[-Body] <string>] [-Supress <bool>] [-Wrap] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Use this command for pre-rendered payloads. Prefer Send-TeamsMessage for typed message models.

## EXAMPLES

### EXAMPLE 1
```powershell
$json | Send-TeamsMessageBody -Uri $workflowUrl
```


## PARAMETERS

### -Body
Pre-rendered JSON body to send.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Proxy
HTTP proxy used for provider requests.

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

### -Supress
Suppresses the rendered JSON output after processing. The default is true.

```yaml
Type: Boolean
Parameter Sets: __AllParameterSets
Aliases: Suppress
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
HTTP request timeout in seconds.

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

### -Uri
HTTPS Teams incoming webhook or Workflows URL.

```yaml
Type: Uri
Parameter Sets: __AllParameterSets
Aliases: TeamsID, Url
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserAgent
Optional product user-agent sent with provider requests.

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

### -Wrap
Wraps an Adaptive Card attachment body in the Teams message envelope.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `System.String`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None
