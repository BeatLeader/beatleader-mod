using System;
using System.Runtime.CompilerServices;

namespace BeatLeader.UI.Reactive {
    internal interface IObservableHost {
        void AddCallback<T>(string propertyName, Action<T> callback);
        void RemoveCallback<T>(string propertyName, Action<T> callback);
        void NotifyPropertyChanged([CallerMemberName] string? name = null);
    }
}