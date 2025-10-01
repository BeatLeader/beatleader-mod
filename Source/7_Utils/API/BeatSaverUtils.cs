using static BeatLeader.BeatSaverAPI.BeatSaverConstants;

namespace BeatLeader.Utils {
    internal static class BeatSaverUtils {
        public static string CreateMapUrl(string mapHash) {
            return BEATSAVER_API_URL + MAPS_HASH_ENDPOINT + mapHash;
        }

        public static string CreateDownloadMapUrl(string mapHash) {
            return $"{BEATSAVER_CDN_URL}{mapHash.ToLower()}.zip";
        }

        public static string CreateMapPageUrl(string bsr) {
            return $"{BEATSAVER_WEBSITE_URL}{MAPS_ENDPOINT}{bsr}";
        }

        public static string FormatBeatmapFolderName(string? id, string? songName, string? authorName, string? hash) {
            return $"{id} ({songName} - {authorName}) [{hash}]";
        }
    }
}