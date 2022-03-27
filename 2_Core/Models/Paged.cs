namespace BeatLeader.Models {
    internal class Paged<T> {
        public Metadata metadata;
        public T data;
    }

    internal class Metadata {
        public int itemsPerPage;
        public int page;
        public int total;
    }
}