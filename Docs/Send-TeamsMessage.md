---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version: https://github.com/EvotecIT/PSTeams
schema: 2.0.0
---
# Send-TeamsMessage
## SYNOPSIS
Sends a typed or legacy-composed message to Microsoft Teams.

## SYNTAX
### TypedMessage
```powershell
Send-TeamsMessage [-Message] <TeamsMessageRequest> [-Target] <TeamsMessageTarget> [-PassThru] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TypedHeroCard
```powershell
Send-TeamsMessage [-HeroCard] <TeamsHeroCard> [-Target] <TeamsMessageTarget> [-PassThru] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TypedThumbnailCard
```powershell
Send-TeamsMessage [-ThumbnailCard] <TeamsThumbnailCard> [-Target] <TeamsMessageTarget> [-PassThru] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TypedListCard
```powershell
Send-TeamsMessage [-ListCard] <TeamsListCard> [-Target] <TeamsMessageTarget> [-PassThru] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### LegacyScript
```powershell
Send-TeamsMessage [[-SectionsInput] <scriptblock>] -Uri <uri> [-MessageTitle <string>] [-MessageText <string>] [-MessageSummary <string>] [-Color <string>] [-HideOriginalBody] [-Proxy <uri>] [-Suppress <bool>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### LegacySections
```powershell
Send-TeamsMessage [[-SectionsInput] <scriptblock>] -Uri <uri> -Sections <TeamsMessageSection[]> [-MessageTitle <string>] [-MessageText <string>] [-MessageSummary <string>] [-Color <string>] [-HideOriginalBody] [-Proxy <uri>] [-Suppress <bool>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Typed parameter sets accept MessageX.Teams message models and targets. Legacy parameter sets preserve the PSTeams composition syntax.
Workflows webhooks are the recommended notification transport for new automation.

## EXAMPLES

### EXAMPLE 1
```powershell
$message = New-TeamsMessage -Title 'Build failed' -Text 'Pipeline 42'; $target = New-TeamsWebhookTarget -Uri $workflowUrl -Workflow; Send-TeamsMessage -Message $message -Target $target
```


### EXAMPLE 2
```powershell
Send-TeamsMessage -Uri $webhookUrl -MessageTitle 'Build failed' -MessageText 'Pipeline 42' -Suppress:$false -WhatIf
```


## PARAMETERS

### -Color
Theme color name or hexadecimal value for the legacy connector card.

```yaml
Type: String
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeroCard
Typed Teams HeroCard to send.

```yaml
Type: TeamsHeroCard
Parameter Sets: TypedHeroCard
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HideOriginalBody
Requests Teams to hide the original message body when supported.

```yaml
Type: SwitchParameter
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ListCard
Typed Teams ListCard to send.

```yaml
Type: TeamsListCard
Parameter Sets: TypedListCard
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Message
Typed Teams message request to send.

```yaml
Type: TeamsMessageRequest
Parameter Sets: TypedMessage
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MessageSummary
Legacy connector-card summary used by notifications and accessibility clients.

```yaml
Type: String
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MessageText
Legacy connector-card body text.

```yaml
Type: String
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MessageTitle
Legacy connector-card title.

```yaml
Type: String
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Returns the typed delivery result.

```yaml
Type: SwitchParameter
Parameter Sets: TypedMessage, TypedHeroCard, TypedThumbnailCard, TypedListCard
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Proxy
HTTP proxy used by the legacy webhook request.

```yaml
Type: Uri
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Sections
Pre-built connector-card sections for the legacy sections parameter set.

```yaml
Type: TeamsMessageSection[]
Parameter Sets: LegacySections
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SectionsInput
Legacy composition script block that emits Teams sections.

```yaml
Type: ScriptBlock
Parameter Sets: LegacyScript, LegacySections
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Suppress
Suppresses the rendered legacy JSON output after processing. The default is true.

```yaml
Type: Boolean
Parameter Sets: LegacyScript, LegacySections
Aliases: Supress
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Typed Teams delivery target.

```yaml
Type: TeamsMessageTarget
Parameter Sets: TypedMessage, TypedHeroCard, TypedThumbnailCard, TypedListCard
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThumbnailCard
Typed Teams ThumbnailCard to send.

```yaml
Type: TeamsThumbnailCard
Parameter Sets: TypedThumbnailCard
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Uri
HTTPS Teams incoming webhook URL used by the legacy parameter sets.

```yaml
Type: Uri
Parameter Sets: LegacyScript, LegacySections
Aliases: TeamsID, Url
Possible values:

Required: True
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
- `System.String`

## RELATED LINKS

- None
