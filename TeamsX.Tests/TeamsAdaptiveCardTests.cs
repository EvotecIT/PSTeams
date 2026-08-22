using TeamsX;

namespace TeamsX.Tests;

public class TeamsAdaptiveCardTests {
    [Fact]
    public void AdaptiveCardStartsWithExpectedDefaults() {
        var card = new TeamsAdaptiveCard();

        Assert.Equal("http://adaptivecards.io/schemas/adaptive-card.json", card.Schema);
        Assert.Equal("AdaptiveCard", card.Type);
        Assert.Equal("1.2", card.Version);
        Assert.Empty(card.Body);
    }

    [Fact]
    public void ContainerAndFactSetCanHoldNestedContent() {
        var container = new TeamsAdaptiveContainer();
        container.Items.Add(new TeamsAdaptiveTextBlock { Text = "Header" });
        container.Items.Add(new TeamsAdaptiveFactSet {
            Facts = {
                new TeamsAdaptiveFact { Title = "Status", Value = "Failed" }
            }
        });

        Assert.Equal(2, container.Items.Count);
        Assert.IsType<TeamsAdaptiveFactSet>(container.Items[1]);
    }

    [Fact]
    public void CardCanHoldColumnsAndActions() {
        var card = new TeamsAdaptiveCard();
        card.Body.Add(new TeamsAdaptiveColumnSet {
            Columns = {
                new TeamsAdaptiveColumn {
                    Width = "stretch",
                    Items = {
                        new TeamsAdaptiveTextBlock { Text = "Left" }
                    }
                },
                new TeamsAdaptiveColumn {
                    Width = "auto",
                    Items = {
                        new TeamsAdaptiveImage { Url = "https://example.test/icon.png" }
                    }
                }
            }
        });
        card.Actions.Add(new TeamsAdaptiveOpenUrlAction {
            Title = "Open build",
            Url = "https://example.test/build/42"
        });

        Assert.Single(card.Actions);
        Assert.IsType<TeamsAdaptiveColumnSet>(card.Body[0]);
    }

    [Fact]
    public void CardCanHoldSubmitAndShowCardActions() {
        var card = new TeamsAdaptiveCard();
        card.Actions.Add(new TeamsAdaptiveSubmitAction { Title = "Approve" });
        card.Actions.Add(new TeamsAdaptiveShowCardAction {
            Title = "Details",
            Card = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveTextBlock { Text = "Nested details" }
                }
            }
        });

        Assert.Equal(2, card.Actions.Count);
        Assert.IsType<TeamsAdaptiveSubmitAction>(card.Actions[0]);
        var showCard = Assert.IsType<TeamsAdaptiveShowCardAction>(card.Actions[1]);
        Assert.IsType<TeamsAdaptiveTextBlock>(Assert.Single(showCard.Card!.Body));
    }

    [Fact]
    public void NestedShowCardImageUsesAdaptiveCardAltTextProperty() {
        var showCard = new TeamsAdaptiveShowCardAction {
            Card = new TeamsAdaptiveCard {
                Body = {
                    new TeamsAdaptiveImage {
                        Url = "https://example.test/status.png",
                        AltText = "Build status"
                    }
                }
            }
        };

        var normalized = Assert.IsType<Dictionary<string, object?>>(TeamsLegacyAdaptiveNormalizer.Normalize(showCard));
        var card = Assert.IsType<Dictionary<string, object?>>(normalized["card"]);
        var body = Assert.IsType<List<object?>>(card["body"]);
        var image = Assert.IsType<Dictionary<string, object?>>(Assert.Single(body));

        Assert.Equal("Build status", image["altText"]);
        Assert.False(image.ContainsKey("alt"));
    }
}
