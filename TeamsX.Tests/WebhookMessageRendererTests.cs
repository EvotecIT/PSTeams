using TeamsX;

namespace TeamsX.Tests;

public class WebhookMessageRendererTests {
    [Fact]
    public void RenderUsesSummaryFallbacks() {
        var request = new TeamsMessageRequest {
            Title = "Build failed",
            Text = "Pipeline 42"
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"summary\":\"Build failed\"", json);
        Assert.Contains("\"title\":\"Build failed\"", json);
        Assert.Contains("\"text\":\"Pipeline 42\"", json);
    }

    [Fact]
    public void RenderSupportsConnectorCardSectionsFactsAndButtons() {
        var request = new TeamsMessageRequest {
            Title = "Build failed",
            Text = "Pipeline 42",
            ThemeColor = "#1E90FF",
            UseConnectorCardFormat = true
        };
        request.Sections.Add(new TeamsMessageSection {
            Title = "Build summary",
            ActivityText = "Pipeline failed",
            Facts = {
                new TeamsMessageFact { Name = "Status", Value = "Failed" }
            },
            Buttons = {
                new TeamsMessageButton {
                    Name = "Open build",
                    Link = "https://example.test/build/42",
                    ButtonType = TeamsMessageButtonType.OpenUri
                }
            }
        });

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"themeColor\":\"#1E90FF\"", json);
        Assert.Contains("\"sections\":[", json);
        Assert.Contains("\"title\":\"Build summary\"", json);
        Assert.Contains("\"name\":\"Status\"", json);
        Assert.Contains("\"@type\":\"OpenURI\"", json);
        Assert.Contains("\"uri\":\"https://example.test/build/42\"", json);
    }

    [Fact]
    public void RenderSupportsConnectorCardSectionImagesAndHeroImageMarkdown() {
        var request = new TeamsMessageRequest {
            Title = "Build failed",
            UseConnectorCardFormat = true
        };
        request.Sections.Add(new TeamsMessageSection {
            ActivityTitle = "Build title",
            ActivitySubtitle = "Build subtitle",
            ActivityText = "Build text",
            ActivityImage = "https://example.test/activity.png",
            Text = "Original text",
            Images = {
                "https://example.test/image.png"
            },
            HeroImages = {
                "![Hero](https://example.test/hero.png)"
            }
        });

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"activityTitle\":\"Build title\"", json);
        Assert.Contains("\"activitySubtitle\":\"Build subtitle\"", json);
        Assert.Contains("\"activityText\":\"Build text\"", json);
        Assert.Contains("\"activityImage\":\"https://example.test/activity.png\"", json);
        Assert.Contains("\"images\":[{\"image\":\"https://example.test/image.png\"}]", json);
        Assert.Contains("![Hero](https://example.test/hero.png) Original text", json);
    }

    [Fact]
    public void RenderWrapsAdaptiveCardAsAttachmentMessage() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveTextBlock {
                        Text = "Pipeline failed",
                        Weight = "Bolder"
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"type\":\"message\"", json);
        Assert.Contains("\"contentType\":\"application/vnd.microsoft.card.adaptive\"", json);
        Assert.Contains("\"text\":\"Pipeline failed\"", json);
        Assert.Contains("\"weight\":\"Bolder\"", json);
    }

    [Fact]
    public void RenderSupportsFactSetImageAndContainer() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveContainer {
                        Id = "panel",
                        Style = "Emphasis",
                        MinimumHeight = "120px",
                        Bleed = true,
                        VerticalContentAlignment = "center",
                        HorizontalAlignment = "Center",
                        Height = "Stretch",
                        Spacing = "Medium",
                        Separator = true,
                        IsVisible = false,
                        BackgroundImage = new TeamsAdaptiveBackgroundImage {
                            FillMode = "Cover",
                            Url = "https://example.test/background.png"
                        },
                        SelectAction = new TeamsAdaptiveOpenUrlAction {
                            Title = "Open panel",
                            Url = "https://example.test/panel"
                        },
                        Items = {
                            new TeamsAdaptiveImage {
                                Url = "https://example.test/build.png",
                                AltText = "Build result"
                            },
                            new TeamsAdaptiveFactSet {
                                Facts = {
                                    new TeamsAdaptiveFact { Title = "Status", Value = "Failed" },
                                    new TeamsAdaptiveFact { Title = "Run", Value = "42" }
                                }
                            }
                        }
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"type\":\"Container\"", json);
        Assert.Contains("\"id\":\"panel\"", json);
        Assert.Contains("\"style\":\"Emphasis\"", json);
        Assert.Contains("\"minHeight\":\"120px\"", json);
        Assert.Contains("\"bleed\":true", json);
        Assert.Contains("\"verticalContentAlignment\":\"center\"", json);
        Assert.Contains("\"backgroundImage\":{\"fillMode\":\"Cover\",\"url\":\"https://example.test/background.png\"}", json);
        Assert.Contains("\"selectAction\":{\"type\":\"Action.OpenUrl\"", json);
        Assert.Contains("\"title\":\"Open panel\"", json);
        Assert.Contains("\"url\":\"https://example.test/panel\"", json);
        Assert.Contains("\"type\":\"Image\"", json);
        Assert.Contains("\"url\":\"https://example.test/build.png\"", json);
        Assert.Contains("\"type\":\"FactSet\"", json);
        Assert.Contains("\"title\":\"Status\"", json);
        Assert.Contains("\"value\":\"Failed\"", json);
    }

    [Fact]
    public void RenderSupportsColumnSetAndOpenUrlAction() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveColumnSet {
                        Style = "Good",
                        MinimumHeight = "80px",
                        Bleed = true,
                        HorizontalAlignment = "Center",
                        Height = "Stretch",
                        Spacing = "Medium",
                        Separator = true,
                        Columns = {
                            new TeamsAdaptiveColumn {
                                Width = "stretch",
                                Height = "Stretch",
                                MinimumHeight = "60px",
                                HorizontalAlignment = "Right",
                                VerticalContentAlignment = "Bottom",
                                Spacing = "Small",
                                Style = "Attention",
                                IsVisible = false,
                                Separator = true,
                                SelectAction = new TeamsAdaptiveToggleVisibilityAction {
                                    Id = "toggle-column",
                                    Title = "Toggle column",
                                    TargetElements = {
                                        "detailsBlock"
                                    }
                                },
                                Items = {
                                    new TeamsAdaptiveTextBlock { Text = "Pipeline failed" }
                                }
                            },
                            new TeamsAdaptiveColumn {
                                Width = "auto",
                                Items = {
                                    new TeamsAdaptiveImage { Url = "https://example.test/status.png" }
                                }
                            }
                        }
                    },
                    new TeamsAdaptiveActionSet {
                        Actions = {
                            new TeamsAdaptiveOpenUrlAction {
                                Title = "Open build",
                                Url = "https://example.test/build/42"
                            }
                        }
                    }
                },
                Actions = {
                    new TeamsAdaptiveOpenUrlAction {
                        Title = "Open build",
                        Url = "https://example.test/build/42"
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"type\":\"ColumnSet\"", json);
        Assert.Contains("\"style\":\"Good\"", json);
        Assert.Contains("\"minHeight\":\"80px\"", json);
        Assert.Contains("\"bleed\":true", json);
        Assert.Contains("\"type\":\"Column\"", json);
        Assert.Contains("\"width\":\"stretch\"", json);
        Assert.Contains("\"verticalContentAlignment\":\"Bottom\"", json);
        Assert.Contains("\"selectAction\":{\"type\":\"Action.ToggleVisibility\",\"id\":\"toggle-column\",\"title\":\"Toggle column\",\"targetElements\":[\"detailsBlock\"]}", json);
        Assert.Contains("\"type\":\"ActionSet\"", json);
        Assert.Contains("\"type\":\"Action.OpenUrl\"", json);
        Assert.Contains("\"url\":\"https://example.test/build/42\"", json);
    }

    [Fact]
    public void RenderSupportsAdaptiveMentionsInMsTeamsEntities() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveTextBlock {
                        Text = "Hello <at>Przemyslaw Klys</at>"
                    }
                },
                Mentions = {
                    new TeamsAdaptiveMention {
                        Text = "<at>Przemyslaw Klys</at>",
                        Mentioned = new TeamsMentionedIdentity {
                            Id = "przemyslaw.klys@example.test",
                            Name = "Przemyslaw Klys"
                        }
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"msteams\":{\"entities\":[", json);
        Assert.Contains("\"type\":\"mention\"", json);
        Assert.Contains("\"id\":\"przemyslaw.klys@example.test\"", json);
        Assert.Contains("\"name\":\"Przemyslaw Klys\"", json);
    }

    [Fact]
    public void RenderSupportsAdaptiveMedia() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveMedia {
                        Poster = "https://example.test/poster.png",
                        AltText = "Build walkthrough",
                        Sources = {
                            new TeamsAdaptiveMediaSource {
                                MimeType = "video/mp4",
                                Url = "https://example.test/video.mp4"
                            },
                            new TeamsAdaptiveMediaSource {
                                MimeType = "video/webm",
                                Url = "https://example.test/video.webm"
                            }
                        }
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"type\":\"Media\"", json);
        Assert.Contains("\"poster\":\"https://example.test/poster.png\"", json);
        Assert.Contains("\"mimeType\":\"video/mp4\"", json);
        Assert.Contains("\"mimeType\":\"video/webm\"", json);
    }

    [Fact]
    public void RenderSupportsImageSetAndToggleVisibilityAction() {
        var request = new TeamsMessageRequest {
            Summary = "Build notification",
            AdaptiveCard = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveImageSet {
                        ImageSize = "Medium",
                        Images = {
                            new TeamsAdaptiveImage { Url = "https://example.test/image-1.png", AltText = "First" },
                            new TeamsAdaptiveImage { Url = "https://example.test/image-2.png", AltText = "Second" }
                        }
                    }
                },
                Actions = {
                    new TeamsAdaptiveToggleVisibilityAction {
                        Title = "Toggle details",
                        TargetElements = {
                            "detailsBlock",
                            "detailsFactSet"
                        }
                    }
                }
            }
        };

        var json = WebhookMessageRenderer.Render(request);

        Assert.Contains("\"type\":\"ImageSet\"", json);
        Assert.Contains("\"imageSize\":\"Medium\"", json);
        Assert.Contains("\"type\":\"Action.ToggleVisibility\"", json);
        Assert.Contains("\"targetElements\":[\"detailsBlock\",\"detailsFactSet\"]", json);
    }

    [Fact]
    public void WrapperCardRendererSupportsTypedWrapperModels() {
        var heroJson = TeamsWrapperCardRenderer.Render(new TeamsHeroCard {
            Title = "Hero"
        });

        var thumbnailJson = TeamsWrapperCardRenderer.Render(new TeamsThumbnailCard {
            Title = "Thumb"
        });

        var listCard = new TeamsListCard {
            Title = "List"
        };
        listCard.Items.Add(new TeamsListCardItem {
            Kind = TeamsListCardItemKind.ResultItem,
            Title = "Item"
        });

        var listJson = TeamsWrapperCardRenderer.Render(listCard);

        Assert.Contains("\"contentType\":\"application/vnd.microsoft.card.hero\"", heroJson);
        Assert.Contains("\"contentType\":\"application/vnd.microsoft.card.thumbnail\"", thumbnailJson);
        Assert.Contains("\"contentType\":\"application/vnd.microsoft.teams.card.list\"", listJson);
    }
}
