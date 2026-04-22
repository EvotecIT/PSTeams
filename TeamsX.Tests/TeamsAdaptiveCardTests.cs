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
            Card = new Dictionary<string, object?> {
                ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json",
                ["type"] = "AdaptiveCard",
                ["version"] = "1.2",
                ["body"] = new object[] {
                    new TeamsAdaptiveTextBlock { Text = "Nested details" }
                }
            }
        });

        Assert.Equal(2, card.Actions.Count);
        Assert.IsType<TeamsAdaptiveSubmitAction>(card.Actions[0]);
        Assert.IsType<TeamsAdaptiveShowCardAction>(card.Actions[1]);
    }
}
