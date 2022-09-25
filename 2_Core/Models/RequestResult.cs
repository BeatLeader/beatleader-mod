
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
