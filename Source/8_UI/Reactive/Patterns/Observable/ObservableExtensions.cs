using System;
using System.Runtime.CompilerServices;

namespace BeatLeader.UI.Reactive {
    internal static class ObservableExtensions {
        public static Observable<T> Observe<T>(this T comp, [CallerMemberName] string name = "") {
            return new() {
                value = comp, propertyName = name
            };
        }

        public static T WithListener<T, TValue>(
            this T host,
            Func<T, Observable<TValue>> predicate,
            Action<TValue> listener
        ) where T : IObservableHost {
            var observable = predicate(host);
            var name = observable.propertyName;
            if (name == null) return host;
            host.AddCallback(name, listener);
            return host;
        }
    }
}