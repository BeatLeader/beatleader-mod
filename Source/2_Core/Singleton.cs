using System;

namespace BeatLeader {
    public class Singleton<T> {
        public static T Instance => _instance ??= _instanceFactory();

        private static T? _instance;
        private static Func<T> _instanceFactory = Activator.CreateInstance<T>;

        protected static void AssignFactory(Func<T> factory) {
            _instanceFactory = factory;
        }
    }
}