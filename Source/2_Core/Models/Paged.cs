using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    public class Paged<T> {
        public Metadata metadata;
        public List<T> data;
        public T? selection;
    }

    public class Metadata {
        public int itemsPerPage;
        public int page;
        public int total;

        public int PagesCount => Mathf.CeilToInt((float)total / itemsPerPage);
    }
}