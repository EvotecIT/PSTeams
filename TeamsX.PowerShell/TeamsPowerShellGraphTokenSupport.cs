using System.Runtime.InteropServices;
using System.Security;

namespace TeamsX.PowerShell;

internal static class TeamsPowerShellGraphTokenSupport {
    public static string ReadEnvironmentVariable(string variableName) {
        if (string.IsNullOrWhiteSpace(variableName)) {
            throw new InvalidOperationException("Environment variable name cannot be null or whitespace.");
        }

        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value)) {
            throw new InvalidOperationException($"Environment variable '{variableName}' does not contain a usable Graph access token.");
        }

        return value;
    }

    public static string ConvertToUnsecureString(SecureString secureString) {
        if (secureString is null) {
            throw new InvalidOperationException("Secure access token cannot be null.");
        }

        var pointer = IntPtr.Zero;
        try {
            pointer = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(pointer) ?? string.Empty;
        } finally {
            if (pointer != IntPtr.Zero) {
                Marshal.ZeroFreeGlobalAllocUnicode(pointer);
            }
        }
    }
}
