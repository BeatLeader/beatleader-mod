namespace BeatLeader.UI.Reactive {
    internal struct Observable<T> {
        public T? value;
        public string? propertyName;

        public static implicit operator Observable<T>(T value) {
            return new() {
                value = value
            };
        }

        public static implicit operator T?(Observable<T> value) {
            return value.value;
        }
    }
}