using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Models
{
    internal class Paged<T>
    {
        public Metadata metadata;
        public T data;
    }

    internal class Metadata
    {
        public int itemsPerPage;
        public int page;
        public int total;
    }
}
