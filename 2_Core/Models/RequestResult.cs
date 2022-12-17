
namespace BeatLeader.Models {
    public readonly struct RequestResult<TResult> {
        public readonly bool isError;
        public readonly TResult value;

        public RequestResult(bool isError, TResult value) {
            this.isError = isError;
            this.value = value;
        }
    }
}
