using System.Threading.Tasks;

namespace BeatLeader.Utils {
    internal static class TaskExtensions {
        public static T RunCatching<T>(this T task) where T : Task {
            task.ContinueWith(
                static x => {
                    if (!x.IsFaulted) return;
                    Plugin.Log.Critical(x.Exception!);
                }
            ).ConfigureAwait(true);
            return task;
        }
    }
}