namespace BeatLeader.WebRequests {
    public enum RequestState {
        Uninitialized,
        Started,
        Parsing,
        Finished,
        Cancelled,
        Failed
    }
}