using System.Collections.Generic;

namespace BeatLeader.Models {

    internal class Paged<T> {
        public Metadata metadata;
        public List<T> data;
        public T? selection;
    }

    internal class Metadata {
        public int itemsPerPage;
        public int page;
        public int total;
    }
}