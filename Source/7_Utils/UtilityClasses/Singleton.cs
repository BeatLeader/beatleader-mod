using System;

namespace BeatLeader {
    internal class Singleton<T> {
        public static T Instance => _instance ??= Activator.CreateInstance<T>();

        private static T? _instance;
    }
}