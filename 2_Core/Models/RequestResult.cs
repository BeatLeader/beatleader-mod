using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Models
{
    public struct RequestResult<TResult>
    {
        public readonly bool isError;
        public readonly TResult value;

        public RequestResult(bool isError, TResult value)
        {
            this.isError = isError;
            this.value = value;
        }
    }
}
