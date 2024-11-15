namespace BeatLeader {
    public struct Optional<T> {
        public T? Value { get; private set; }
        public bool HasValue { get; private set; }

        public void SetValueIfNotSet(T newValue) {
            if (HasValue) return;
            Value = newValue;
            HasValue = true;
        }

        public T GetValueOrDefault(T defaultValue) {
            return HasValue ? Value! : defaultValue;
        }

        public static implicit operator bool(Optional<T> value) {
            return value.HasValue;
        }

        public static implicit operator T?(Optional<T> value) {
            return value.Value;
        }

        public static implicit operator Optional<T>(T? value) {
            return new() {
                Value = value,
                HasValue = true
            };
        }
    }
}