using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Tasks.TaskStatus;

namespace BeatLeader;

/// <summary>
/// Ensures that only the latest task is the leading one.
/// </summary>
internal class TaskGate {
    public CancellationToken Token => _tokenSource.Token;

    private CancellationTokenSource _tokenSource = new();
    private Task? _currentTask;

    public void ThrowIfCancellationRequested() {
        Token.ThrowIfCancellationRequested();
    }
    
    public void Cancel() {
        // Check if the task is running
        if (_currentTask?.Status is
            not Canceled
            and not Faulted
            and not RanToCompletion
        ) {
            _tokenSource.Cancel();
            _tokenSource = new();
        }
    }

    public void SetTask(Task task) {
        Cancel();
        _currentTask = task;
    }
}