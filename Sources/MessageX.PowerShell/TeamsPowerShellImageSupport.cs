using System.IO;
using System.Linq;
using MessageX.Teams;

namespace MessageX.PowerShell;

internal static class TeamsPowerShellImageSupport {
    public static void ValidateImageFile(FileInfo path, string parameterName, string missingMessage, string extensionMessage) {
        if (!path.Exists) {
            throw new FileNotFoundException(missingMessage, path.FullName);
        }

        var extension = path.Extension;
        if (!string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException(extensionMessage, parameterName);
        }
    }

    public static string ResolveImageFile(FileInfo path) {
        return TeamsImageDataUtility.FromFile(path.FullName);
    }

    public static string ResolveBuiltInImage(string imageName) {
        var assemblyDirectory = Path.GetDirectoryName(typeof(TeamsPowerShellImageSupport).Assembly.Location) ?? string.Empty;
        var candidates = new[] {
            Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "Images", $"{imageName}.jpg")),
            Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", "Module", "PSTeams", "Images", $"{imageName}.jpg"))
        };

        var imagePath = candidates.FirstOrDefault(File.Exists);
        if (imagePath is null) {
            return string.Empty;
        }

        return TeamsImageDataUtility.FromFile(imagePath);
    }
}
