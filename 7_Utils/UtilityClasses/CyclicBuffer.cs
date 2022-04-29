namespace BeatLeader {
    public class CyclicBuffer<T> {
        public CyclicBuffer(
            int capacity
        ) {
            _capacity = capacity;
            _nodes = new T[capacity];
            _buffer = new T[capacity];
        }

        private readonly T[] _nodes;
        private readonly T[] _buffer;

        private readonly int _capacity;
        private int _nextElementIndex;
        private int _lastIndex;
        public int Size;

        public bool Add(T point) {
            _lastIndex = _nextElementIndex;
            _nodes[_lastIndex] = point;

            _nextElementIndex -= 1;
            if (_nextElementIndex < 0) _nextElementIndex = _capacity - 1;

            if (Size >= _capacity) return true;
            Size += 1;
            return false;
        }

        public T[] GetBuffer() {
            var bufferIndex = 0;

            for (var i = _lastIndex; i < _capacity; i++) {
                _buffer[bufferIndex++] = _nodes[i];
            }

            for (var i = 0; i < _lastIndex; i++) {
                _buffer[bufferIndex++] = _nodes[i];
            }

            return _buffer;
        }
    }
}