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
}
