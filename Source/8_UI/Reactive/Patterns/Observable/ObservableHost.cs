using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Utils;

namespace BeatLeader.UI.Reactive {
    internal class ObservableHost : IObservableHost {
        public ObservableHost(object keeper) {
            _keeper = keeper;
        }

        private readonly Dictionary<string, Delegate> _delegates = new();
        private readonly Dictionary<string, PropertyInfo> _properties = new();
        private readonly object _keeper;

        public void AddCallback<T>(string propertyName, Action<T> callback) {
            if (_delegates.TryGetValue(propertyName, out var del)) {
                del = Delegate.Combine(del, callback);
            } else {
                del = callback;
            }
            _delegates[propertyName] = del;
        }

        public void RemoveCallback<T>(string propertyName, Action<T> callback) {
            if (!_delegates.TryGetValue(propertyName, out var del)) return;
            del = Delegate.Remove(del, callback);
            _delegates[propertyName] = del;
        }

        public void NotifyPropertyChanged(string? name = null) {
            if (name == null || !_delegates.TryGetValue(name, out var del)) return;

            if (!_properties.TryGetValue(name, out var prop)) {
                prop = _keeper.GetType().GetProperty(name, ReflectionUtils.DefaultFlags);
                if (prop == null) return;
                _properties[name] = prop;
            }
            var getter = prop.GetMethod;
            if (getter == null) {
                throw new Exception("Notifiable property must have a getter method to work");
            }

            try {
                var value = getter.Invoke(_keeper, Array.Empty<object>());
                var type = value.GetType();
                if (type.GetGenericTypeDefinition() == typeof(Observable<>)) {
                    var targetType = type.GetGenericArguments()[0];
                    ReflectionUtils.CastValueOp(type, targetType, value, out value);
                }
                del.DynamicInvoke(value);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to invoke callback: \n{ex}");
            }
        }
    }
}