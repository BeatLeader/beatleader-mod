namespace BeatLeader.Models {
    public enum ScoreUploadStatus {
        Uploaded = 1,
        NonPB = 2,
        Attempt = 3,
        Error = 4,
    };

    public class ScoreUploadResponse
    {
        public Score? Score { get; set; }
        public ScoreUploadStatus Status { get; set; }
        public string Description { get; set; }
    }
}