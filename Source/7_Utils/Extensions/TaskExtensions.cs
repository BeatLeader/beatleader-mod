using System.Threading.Tasks;

namespace BeatLeader.Utils {
    internal static class TaskExtensions {
        public static void RunCatching(this Task task) {
            task.ContinueWith(
                static x => {
                    if (!x.IsFaulted) return;
                    Plugin.Log.Critical(x.Exception!);
                }
            ).ConfigureAwait(true);
        }
    }
}