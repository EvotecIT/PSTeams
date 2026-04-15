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
                        Columns = {
                            new TeamsAdaptiveColumn {
                                Width = "stretch",
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
        Assert.Contains("\"type\":\"Column\"", json);
        Assert.Contains("\"width\":\"stretch\"", json);
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
}
