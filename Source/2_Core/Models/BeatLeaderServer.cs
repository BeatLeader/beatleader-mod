using System.Collections.Generic;

namespace BeatLeader.Models {
    internal enum BeatLeaderServer {
        COM_DOMAIN,
        NET_DOMAIN,
        XYZ_DOMAIN
    }

    internal static class BeatLeaderServerUtils {
        public static readonly List<BeatLeaderServer> ServerOptions = new List<BeatLeaderServer>() {
            BeatLeaderServer.COM_DOMAIN,
            BeatLeaderServer.NET_DOMAIN
        };
        
        public static string GetName(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                case BeatLeaderServer.XYZ_DOMAIN:
                default: return "beatleader.com";
            }
        }

        public static string GetAPIUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://api.beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                case BeatLeaderServer.XYZ_DOMAIN:
                default: return "https://api.beatleader.com";
            }
        }

        public static string GetWebsiteUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                case BeatLeaderServer.XYZ_DOMAIN:
                default: return "https://beatleader.com";
            }
        }
    }
}