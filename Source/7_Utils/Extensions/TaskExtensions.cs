using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Utils {
    internal static class TaskExtensions {
        public static T RunCatching<T>(this T task) where T : Task {
            task.ContinueWith(
                static x => {
                    if (!x.IsFaulted) return;
                    Plugin.Log.Critical(x.Exception!);
                }
            );
            return task;
        }

        public static Task RunOnMainThread(Action callback) {
            var completionSource = new TaskCompletionSource<bool>();

            SynchronizationContext.Current.Send(
                _ => {
                    try {
                        callback();
                        completionSource.SetResult(true);
                    } catch (Exception ex) {
                        completionSource.SetException(ex);
                    }
                },
                null
            );

            return completionSource.Task;
        }
    }
}