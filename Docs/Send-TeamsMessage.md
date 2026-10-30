---
external help file: MessageX-help.xml
Module Name: MessageX
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Send-TeamsMessage
## SYNOPSIS
Sends an Adaptive Card through a Teams Workflow webhook.

## SYNTAX
### __AllParameterSets
```powershell
Send-TeamsMessage [-Message] <TeamsMessageRequest> [-Target] <TeamsMessageTarget> [-PassThru] [-Proxy <uri>] [-TimeoutSeconds <int>] [-UserAgent <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sends an Adaptive Card through a Teams Workflow webhook.

## EXAMPLES

### EXAMPLE 1
```powershell
$target = New-TeamsWebhookTarget -Uri $workflowUrl -Destination Channel; $card = New-TeamsAdaptiveCard -Body { New-TeamsAdaptiveTextBlock -Text 'Build failed' }; $message = New-TeamsMessage -AdaptiveCard $card -Summary 'Build failed'; Send-TeamsMessage -Message $message -Target $target
```


## PARAMETERS

### -Message
Specifies a value for message.

```yaml
Type: TeamsMessageRequest
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Specifies the pass thru switch.

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

### -Target
Specifies a value for target.

```yaml
Type: TeamsMessageTarget
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
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
Default value: 100 (valid range: 1-3600)
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

- `None`

## OUTPUTS

- `MessageX.Teams.TeamsDeliveryResult`

## RELATED LINKS

- None
