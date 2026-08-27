---
Module Name: PSTeams
Module Guid: a46c3b0b-5687-4d62-89c5-753ae01e0926
Download Help Link: https://github.com/EvotecIT/PSTeams
Help Version: 2.4.1
Locale: en-US
---
# PSTeams Module
## Description
PSTeams provides typed Teams, Slack, and Discord message composition and delivery through MessageX libraries and thin compiled PowerShell cmdlets.

## PSTeams Cmdlets
### [Add-DiscordReaction](Add-DiscordReaction.md)
Adds the authenticated Discord bot's reaction to a message.

### [Add-SlackReaction](Add-SlackReaction.md)
Adds the authenticated Slack application's reaction to a message.

### [ConvertTo-DiscordJson](ConvertTo-DiscordJson.md)
Serializes a typed Discord message using the exact provider payload contract.

### [ConvertTo-SlackJson](ConvertTo-SlackJson.md)
Serializes a typed Slack message using the exact provider payload contract.

### [ConvertTo-TeamsFact](ConvertTo-TeamsFact.md)
Converts dictionaries and PowerShell objects into Teams facts.

### [ConvertTo-TeamsJson](ConvertTo-TeamsJson.md)
ConvertTo-TeamsJson [-InputObject] <Object> [<CommonParameters>]

### [ConvertTo-TeamsSection](ConvertTo-TeamsSection.md)
Converts dictionaries and PowerShell objects into Teams sections.

### [Get-DiscordMessage](Get-DiscordMessage.md)
Retrieves an application-owned Discord message through a bot or owning webhook.

### [New-AdaptiveAction](New-AdaptiveAction.md)
Creates a legacy-named adaptive action backed by the MessageX.Teams model.

### [New-AdaptiveActionSet](New-AdaptiveActionSet.md)
Creates a legacy-named adaptive action set backed by the MessageX.Teams model.

### [New-AdaptiveCard](New-AdaptiveCard.md)
Creates a legacy-named adaptive card message backed by the MessageX.Teams model.

### [New-AdaptiveColumn](New-AdaptiveColumn.md)
Creates a legacy-named adaptive column backed by the MessageX.Teams model.

### [New-AdaptiveColumnSet](New-AdaptiveColumnSet.md)
Creates a legacy-named adaptive column set backed by the MessageX.Teams model.

### [New-AdaptiveContainer](New-AdaptiveContainer.md)
Creates a legacy-named adaptive container backed by the MessageX.Teams model.

### [New-AdaptiveFact](New-AdaptiveFact.md)
Creates a legacy-named adaptive fact backed by the MessageX.Teams model.

### [New-AdaptiveFactSet](New-AdaptiveFactSet.md)
Creates a legacy-named adaptive fact set backed by the MessageX.Teams model.

### [New-AdaptiveImage](New-AdaptiveImage.md)
Creates a legacy-named adaptive image backed by the MessageX.Teams model.

### [New-AdaptiveImageSet](New-AdaptiveImageSet.md)
Creates a legacy-named adaptive image set backed by the MessageX.Teams model.

### [New-AdaptiveLineBreak](New-AdaptiveLineBreak.md)
Creates a legacy-named adaptive line break backed by a newline text block.

### [New-AdaptiveMedia](New-AdaptiveMedia.md)
Creates a legacy-named adaptive media element backed by the MessageX.Teams model.

### [New-AdaptiveMediaSource](New-AdaptiveMediaSource.md)
Creates a legacy-named adaptive media source backed by the MessageX.Teams model.

### [New-AdaptiveMention](New-AdaptiveMention.md)
Creates a legacy-named adaptive mention backed by the MessageX.Teams model.

### [New-AdaptiveRichTextBlock](New-AdaptiveRichTextBlock.md)
Creates a legacy-named adaptive rich text block backed by the MessageX.Teams model.

### [New-AdaptiveTable](New-AdaptiveTable.md)
Creates a legacy-named adaptive table by projecting objects into column sets.

### [New-AdaptiveTextBlock](New-AdaptiveTextBlock.md)
Creates a legacy-named adaptive text block backed by the MessageX.Teams model.

### [New-CardList](New-CardList.md)
Creates or sends a Teams ListCard payload.

### [New-CardListButton](New-CardListButton.md)
Creates a button for ListCard, HeroCard, and ThumbnailCard payloads.

### [New-CardListItem](New-CardListItem.md)
Creates one Teams list-card item.

### [New-DiscordActionRow](New-DiscordActionRow.md)
Creates a Discord action row.

### [New-DiscordAllowedMentions](New-DiscordAllowedMentions.md)
Creates an explicit Discord mention policy. The default policy notifies nobody.

### [New-DiscordAttachment](New-DiscordAttachment.md)
Creates an in-memory Discord attachment from a file or byte array.

### [New-DiscordAuthor](New-DiscordAuthor.md)
Creates Discord embed author metadata.

### [New-DiscordButton](New-DiscordButton.md)
Creates a Discord interactive or link button.

### [New-DiscordChannelTarget](New-DiscordChannelTarget.md)
Creates a Discord bot channel target.

### [New-DiscordConnection](New-DiscordConnection.md)
Creates an authenticated Discord bot connection without exposing its token.

### [New-DiscordDirectMessageTarget](New-DiscordDirectMessageTarget.md)
Creates a Discord bot direct-message target.

### [New-DiscordFact](New-DiscordFact.md)
Creates a Discord embed field.

### [New-DiscordFooter](New-DiscordFooter.md)
Creates Discord embed footer metadata.

### [New-DiscordImage](New-DiscordImage.md)
Creates Discord embed image or thumbnail media.

### [New-DiscordMessage](New-DiscordMessage.md)
Creates a provider-native Discord message.

### [New-DiscordModal](New-DiscordModal.md)
Creates a Discord modal for an immediate interaction response.

### [New-DiscordSection](New-DiscordSection.md)
Creates a rich Discord embed.

### [New-DiscordSelectOption](New-DiscordSelectOption.md)
Creates a Discord string-select option.

### [New-DiscordStringSelect](New-DiscordStringSelect.md)
Creates a Discord string select menu.

### [New-DiscordTextInput](New-DiscordTextInput.md)
Creates a Discord modal text input.

### [New-DiscordThreadTarget](New-DiscordThreadTarget.md)
Creates a Discord bot thread-channel target.

### [New-DiscordWebhookTarget](New-DiscordWebhookTarget.md)
Creates a Discord incoming-webhook target, optionally within an existing thread.

### [New-HeroCard](New-HeroCard.md)
Creates or sends a Teams HeroCard payload.

### [New-SlackActions](New-SlackActions.md)
Creates a Slack Block Kit actions block.

### [New-SlackButton](New-SlackButton.md)
Creates a Slack Block Kit button.

### [New-SlackConnection](New-SlackConnection.md)
Creates an authenticated Slack bot connection without exposing its token.

### [New-SlackContext](New-SlackContext.md)
Creates a Slack Block Kit text context row.

### [New-SlackConversationTarget](New-SlackConversationTarget.md)
Creates a Slack channel, direct-message, multiparty-message, or user target.

### [New-SlackDivider](New-SlackDivider.md)
Creates a Slack Block Kit divider.

### [New-SlackHeader](New-SlackHeader.md)
Creates a Slack Block Kit header.

### [New-SlackInput](New-SlackInput.md)
Creates a Slack modal input block.

### [New-SlackMessage](New-SlackMessage.md)
Creates a provider-native Slack message.

### [New-SlackModal](New-SlackModal.md)
Creates a typed Slack modal view.

### [New-SlackPlainTextInput](New-SlackPlainTextInput.md)
Creates a Slack modal plain-text input element.

### [New-SlackSection](New-SlackSection.md)
Creates a Slack Block Kit section.

### [New-SlackText](New-SlackText.md)
Creates typed Slack plain-text or mrkdwn content.

### [New-SlackWebhookTarget](New-SlackWebhookTarget.md)
Creates a fixed-destination Slack incoming-webhook target.

### [New-TeamsActivityImage](New-TeamsActivityImage.md)
Creates a typed activity-image directive for connector-card sections.

### [New-TeamsActivitySubtitle](New-TeamsActivitySubtitle.md)
Creates a typed activity-subtitle directive for connector-card sections.

### [New-TeamsActivityText](New-TeamsActivityText.md)
Creates a typed activity-text directive for connector-card sections.

### [New-TeamsActivityTitle](New-TeamsActivityTitle.md)
Creates a typed activity-title directive for connector-card sections.

### [New-TeamsAdaptiveActionSet](New-TeamsAdaptiveActionSet.md)
New-TeamsAdaptiveActionSet [-Actions <TeamsAdaptiveAction[]>] [<CommonParameters>]

### [New-TeamsAdaptiveCard](New-TeamsAdaptiveCard.md)
New-TeamsAdaptiveCard [-Body <TeamsAdaptiveCardElement[]>] [-Actions <TeamsAdaptiveAction[]>] [-Mentions <TeamsAdaptiveMention[]>] [-Version <string>] [-Refresh <TeamsAdaptiveRefresh>] [-FallbackText <string>] [-MinimumHeight <int>] [-Speak <string>] [-Language <string>] [-VerticalContentAlignment <string>] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-FullWidth] [-AllowImageExpand] [<CommonParameters>]

### [New-TeamsAdaptiveColumn](New-TeamsAdaptiveColumn.md)
New-TeamsAdaptiveColumn [-Width <string>] [-WidthInWeight <int>] [-WidthInPixels <int>] [-Height <string>] [-MinimumHeight <int>] [-HorizontalAlignment <string>] [-VerticalContentAlignment <string>] [-Spacing <string>] [-Style <string>] [-Hidden] [-Separator] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [-Items <TeamsAdaptiveCardElement[]>] [<CommonParameters>]

### [New-TeamsAdaptiveColumnSet](New-TeamsAdaptiveColumnSet.md)
New-TeamsAdaptiveColumnSet [-Columns <TeamsAdaptiveColumn[]>] [-Style <string>] [-MinimumHeight <int>] [-Bleed] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [<CommonParameters>]

### [New-TeamsAdaptiveContainer](New-TeamsAdaptiveContainer.md)
New-TeamsAdaptiveContainer [-Items <TeamsAdaptiveCardElement[]>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Style <string>] [-MinimumHeight <int>] [-Bleed] [-VerticalContentAlignment <string>] [-Id <string>] [-Hidden] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-SelectAction <string>] [-SelectActionId <string>] [-SelectActionUrl <string>] [-SelectActionTitle <string>] [-SelectActionTargetElement <string[]>] [<CommonParameters>]

### [New-TeamsAdaptiveExecuteAction](New-TeamsAdaptiveExecuteAction.md)
Creates a Teams Universal Action model for a bot-capable outbound transport. Current webhook targets reject it.

### [New-TeamsAdaptiveFact](New-TeamsAdaptiveFact.md)
New-TeamsAdaptiveFact [-Title] <string> [-Value] <string> [<CommonParameters>]

### [New-TeamsAdaptiveFactSet](New-TeamsAdaptiveFactSet.md)
New-TeamsAdaptiveFactSet [-Facts <TeamsAdaptiveFact[]>] [<CommonParameters>]

### [New-TeamsAdaptiveImage](New-TeamsAdaptiveImage.md)
New-TeamsAdaptiveImage [-Url] <string> [-AltText <string>] [-Size <string>] [<CommonParameters>]

### [New-TeamsAdaptiveImageSet](New-TeamsAdaptiveImageSet.md)
New-TeamsAdaptiveImageSet -Images <TeamsAdaptiveImage[]> [-ImageSize <string>] [<CommonParameters>]

### [New-TeamsAdaptiveMedia](New-TeamsAdaptiveMedia.md)
New-TeamsAdaptiveMedia -Sources <TeamsAdaptiveMediaSource[]> [-PosterUrl <string>] [-AlternateText <string>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

### [New-TeamsAdaptiveMediaSource](New-TeamsAdaptiveMediaSource.md)
New-TeamsAdaptiveMediaSource [-Type] <string> [-Url] <string> [<CommonParameters>]

### [New-TeamsAdaptiveMention](New-TeamsAdaptiveMention.md)
New-TeamsAdaptiveMention [-Text] <string> [-UserPrincipalName] <string> [[-Name] <string>] [<CommonParameters>]

### [New-TeamsAdaptiveOpenUrlAction](New-TeamsAdaptiveOpenUrlAction.md)
New-TeamsAdaptiveOpenUrlAction [-Title] <string> [-Url] <string> [<CommonParameters>]

### [New-TeamsAdaptiveRefresh](New-TeamsAdaptiveRefresh.md)
Creates an Adaptive Card refresh model for a bot-capable outbound transport. Current webhook targets reject it.

### [New-TeamsAdaptiveRichTextBlock](New-TeamsAdaptiveRichTextBlock.md)
New-TeamsAdaptiveRichTextBlock -Text <string[]> [-Color <string[]>] [-Subtle <bool[]>] [-Size <string[]>] [-Weight <string[]>] [-Highlight <bool[]>] [-Italic <bool[]>] [-StrikeThrough <bool[]>] [-FontType <string[]>] [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

New-TeamsAdaptiveRichTextBlock -Inlines <TeamsAdaptiveTextRun[]> [-Spacing <string>] [-Separator] [-HorizontalAlignment <string>] [-Height <string>] [-Id <string>] [-Hidden] [<CommonParameters>]

### [New-TeamsAdaptiveShowCardAction](New-TeamsAdaptiveShowCardAction.md)
New-TeamsAdaptiveShowCardAction [-Title] <string> [-Id <string>] [-Card <TeamsAdaptiveCard>] [-Body <TeamsAdaptiveCardElement[]>] [-Actions <TeamsAdaptiveAction[]>] [-Mentions <TeamsAdaptiveMention[]>] [-Version <string>] [-FallbackText <string>] [-MinimumHeight <int>] [-Speak <string>] [-Language <string>] [-VerticalContentAlignment <string>] [-BackgroundUrl <string>] [-BackgroundFillMode <string>] [-BackgroundHorizontalAlignment <string>] [-BackgroundVerticalAlignment <string>] [-FullWidth] [-AllowImageExpand] [<CommonParameters>]

### [New-TeamsAdaptiveSubmitAction](New-TeamsAdaptiveSubmitAction.md)
New-TeamsAdaptiveSubmitAction [-Title] <string> [-Id <string>] [-Data <IDictionary>] [<CommonParameters>]

### [New-TeamsAdaptiveTextBlock](New-TeamsAdaptiveTextBlock.md)
New-TeamsAdaptiveTextBlock [-Text] <string> [-NoWrap] [-Size <string>] [-Weight <string>] [-Color <string>] [<CommonParameters>]

### [New-TeamsAdaptiveTextRun](New-TeamsAdaptiveTextRun.md)
New-TeamsAdaptiveTextRun [-Text] <string> [-Color <string>] [-Subtle <Boolean>] [-Size <string>] [-Weight <string>] [-Highlight <Boolean>] [-Italic <Boolean>] [-StrikeThrough <Boolean>] [-FontType <string>] [<CommonParameters>]

### [New-TeamsAdaptiveToggleVisibilityAction](New-TeamsAdaptiveToggleVisibilityAction.md)
New-TeamsAdaptiveToggleVisibilityAction [-Title] <string> [-TargetElementIds] <string[]> [<CommonParameters>]

### [New-TeamsBigImage](New-TeamsBigImage.md)
Creates a hero-style markdown image entry for section text.

### [New-TeamsButton](New-TeamsButton.md)
Creates a connector-card button/action.

### [New-TeamsCardImage](New-TeamsCardImage.md)
Creates an image entry for HeroCard or ThumbnailCard content.

### [New-TeamsFact](New-TeamsFact.md)
Creates a connector-card fact item.

### [New-TeamsHeroCard](New-TeamsHeroCard.md)
New-TeamsHeroCard [-Title <string>] [-SubTitle <string>] [-Text <string>] [-Images <TeamsCardImage[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

### [New-TeamsImage](New-TeamsImage.md)
Creates a standard section image entry.

### [New-TeamsList](New-TeamsList.md)
Builds a legacy list fact from typed list items.

### [New-TeamsListCard](New-TeamsListCard.md)
New-TeamsListCard [-Title <string>] [-Items <TeamsListCardItem[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

### [New-TeamsListItem](New-TeamsListItem.md)
Creates a typed legacy list item for connector-card facts.

### [New-TeamsMessage](New-TeamsMessage.md)
New-TeamsMessage [-Title <string>] [-Text <string>] [-Summary <string>] [-AdaptiveCard <TeamsAdaptiveCard>] [-Sections <TeamsMessageSection[]>] [-ThemeColor <string>] [-HideOriginalBody] [-UseConnectorCardFormat] [<CommonParameters>]

### [New-TeamsSection](New-TeamsSection.md)
Creates a connector-card section.

### [New-TeamsThumbnailCard](New-TeamsThumbnailCard.md)
New-TeamsThumbnailCard [-Title <string>] [-SubTitle <string>] [-Text <string>] [-Images <TeamsCardImage[]>] [-Buttons <TeamsCardButton[]>] [<CommonParameters>]

### [New-TeamsWebhookTarget](New-TeamsWebhookTarget.md)
Creates a send-only Teams incoming webhook or Power Automate Workflow target.

### [New-ThumbnailCard](New-ThumbnailCard.md)
Creates or sends a Teams ThumbnailCard payload.

### [Remove-DiscordMessage](Remove-DiscordMessage.md)
Deletes an application-owned Discord message through a bot or owning webhook.

### [Remove-DiscordReaction](Remove-DiscordReaction.md)
Removes the authenticated Discord bot's reaction from a message.

### [Remove-SlackMessage](Remove-SlackMessage.md)
Deletes an application-owned Slack message.

### [Remove-SlackReaction](Remove-SlackReaction.md)
Removes the authenticated Slack application's reaction from a message.

### [Resolve-SlackConversation](Resolve-SlackConversation.md)
Resolves or opens a Slack direct-message conversation for explicit user identifiers.

### [Send-DiscordMessage](Send-DiscordMessage.md)
Sends simple or typed messages through Discord incoming webhooks or authenticated bot REST.

### [Send-SlackFile](Send-SlackFile.md)
Uploads a file through Slack's external upload workflow.

### [Send-SlackMessage](Send-SlackMessage.md)
Sends simple or typed messages through Slack incoming webhooks or the authenticated Web API.

### [Send-TeamsMessage](Send-TeamsMessage.md)
Sends a typed or legacy-composed message to Microsoft Teams.

### [Send-TeamsMessageBody](Send-TeamsMessageBody.md)
Sends a raw Teams message payload body to an incoming webhook.

### [Test-DiscordInteractionSignature](Test-DiscordInteractionSignature.md)
Verifies a Discord interaction signature and bounds its age. The hosting service must separately
reject duplicate signatures because an age window alone does not prevent replay.

### [Update-DiscordMessage](Update-DiscordMessage.md)
Updates an application-owned Discord message through a bot or owning webhook.

### [Update-SlackMessage](Update-SlackMessage.md)
Updates an application-owned Slack message.
