using System.Collections.Generic;
using UnityEngine;

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

        public int PagesCount => Mathf.CeilToInt((float)total / itemsPerPage);
    }
}