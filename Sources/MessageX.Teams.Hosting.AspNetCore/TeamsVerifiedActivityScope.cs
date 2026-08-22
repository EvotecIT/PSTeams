using System.Threading;
using Microsoft.Teams.Core.Schema;

namespace MessageX.Teams.Hosting.AspNetCore;

internal sealed class TeamsVerifiedActivityScope : IDisposable {
    private static readonly AsyncLocal<CoreActivity?> CurrentActivity = new();
    private readonly CoreActivity? _previous;
    private bool _disposed;

    private TeamsVerifiedActivityScope(CoreActivity activity) {
        _previous = CurrentActivity.Value;
        CurrentActivity.Value = activity;
    }

    public static CoreActivity? Current => CurrentActivity.Value;

    public static TeamsVerifiedActivityScope Push(CoreActivity activity) {
        ArgumentNullException.ThrowIfNull(activity);
        return new TeamsVerifiedActivityScope(activity);
    }

    public void Dispose() {
        if (_disposed) {
            return;
        }
        CurrentActivity.Value = _previous;
        _disposed = true;
    }
}
