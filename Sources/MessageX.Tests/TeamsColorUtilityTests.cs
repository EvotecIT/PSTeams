using MessageX.Teams;

namespace MessageX.Tests;

public class TeamsColorUtilityTests {
    [Fact]
    public void NormalizeToHexSupportsLegacyPSTeamsPaletteNames() {
        var hex = TeamsColorUtility.NormalizeToHex("AlbescentWhite");

        Assert.Equal("#E3DAC9", hex);
    }

    [Fact]
    public void NormalizeToHexSupportsSystemDrawingNamedColors() {
        var hex = TeamsColorUtility.NormalizeToHex("DodgerBlue");

        Assert.Equal("#1E90FF", hex);
    }
}
