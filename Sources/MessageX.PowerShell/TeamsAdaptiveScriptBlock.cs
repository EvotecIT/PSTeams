using System.Management.Automation;

namespace MessageX.PowerShell;

internal static class TeamsAdaptiveScriptBlock {
    internal static IEnumerable<T> Evaluate<T>(ScriptBlock? block) where T : class {
        if (block is null) yield break;
        foreach (var output in block.Invoke()) {
            if (output?.BaseObject is T value) {
                yield return value;
            } else {
                throw new ArgumentException($"The script block must produce {typeof(T).Name} values.");
            }
        }
    }
}
