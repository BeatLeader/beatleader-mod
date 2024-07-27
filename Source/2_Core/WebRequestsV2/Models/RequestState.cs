namespace BeatLeader.WebRequests {
    public enum RequestState {
        Uninitialized,
        Uploading,
        Downloading,
        Parsing,
        Finished,
        Cancelled,
        Failed
    }
}