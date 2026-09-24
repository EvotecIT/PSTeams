using MessageX.Teams;

namespace MessageX.Tests;

public sealed class TeamsImageDataUtilityTests {
    [Theory]
    [InlineData("89504E470D0A1A0A", "image/png")]
    [InlineData("FFD8FFE0", "image/jpeg")]
    public void InlineMediaTypeFollowsBytesRatherThanMisleadingExtension(string hexadecimal, string mediaType) {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jpg");
        var bytes = Convert.FromHexString(hexadecimal);
        try {
            File.WriteAllBytes(path, bytes);
            Assert.Equal($"data:{mediaType};base64,{Convert.ToBase64String(bytes)}", TeamsImageDataUtility.FromFile(path));
        } finally {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("not an image")]
    public void UnrecognizedImageDataIsRejected(string content) {
        var path = Path.GetTempFileName();
        try {
            File.WriteAllText(path, content);
            Assert.Throws<ArgumentException>(() => TeamsImageDataUtility.FromFile(path));
        } finally {
            File.Delete(path);
        }
    }
}
