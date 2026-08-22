---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Remove-SlackReaction
## SYNOPSIS
Removes the authenticated Slack application's reaction from a message.

## SYNTAX
### __AllParameterSets
```powershell
Remove-SlackReaction [-Reference] <MessageReference> [-Reaction] <string> -Connection <SlackConnection> [-PassThru] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes the authenticated Slack application's reaction from a message.

## EXAMPLES

### EXAMPLE 1
```powershell
Remove-SlackReaction -Connection 'Value'
```


## PARAMETERS

### -Connection
Authenticated Slack Web API connection.

```yaml
Type: SlackConnection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Returns the typed operation result.

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

### -Reaction
Slack reaction name without surrounding colons.

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

### -Reference
Durable Slack message reference.

```yaml
Type: MessageReference
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `MessageX.Core.MessageReference`

## OUTPUTS

- `MessageX.Slack.SlackDeliveryResult`

## RELATED LINKS

- None
