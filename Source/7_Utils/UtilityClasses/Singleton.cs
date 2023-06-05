using System;

namespace BeatLeader {
    public class Singleton<T, TImpl> where TImpl : class, T {
        public static T Instance => _instance ??= Activator.CreateInstance<TImpl>();

        private static T? _instance;
    }

    public class Singleton<T> : Singleton<T, T> where T : class { }
}