using System.Runtime.InteropServices;
using System.Security;

namespace MessageX.PowerShell;

internal static class SecureStringSupport {
    public static TResult Use<TResult>(SecureString value, Func<string, TResult> action) {
        if (value is null) {
            throw new ArgumentNullException(nameof(value));
        }
        if (action is null) {
            throw new ArgumentNullException(nameof(action));
        }
        var pointer = IntPtr.Zero;
        try {
            pointer = Marshal.SecureStringToBSTR(value);
            return action(Marshal.PtrToStringBSTR(pointer) ?? string.Empty);
        } finally {
            if (pointer != IntPtr.Zero) {
                Marshal.ZeroFreeBSTR(pointer);
            }
        }
    }
}
