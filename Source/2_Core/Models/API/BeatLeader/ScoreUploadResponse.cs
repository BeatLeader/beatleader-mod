using Newtonsoft.Json;

namespace BeatLeader.Models {
    public enum ScoreUploadStatus {
        Uploaded = 1,
        NonPB = 2,
        Attempt = 3,
        Error = 4,
    };

    public class ScoreUploadResponse {
        [JsonProperty("score")]
        public Score Score;
        [JsonProperty("status")]
        public ScoreUploadStatus Status;
        [JsonProperty("description")]
        public string Description;
    }
}