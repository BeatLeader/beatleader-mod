using BeatLeader.Models;

namespace BeatLeader.Utils
{
    // TODO: Replace by smth not static to store app context
    internal class BLContext
    {
        public static Player? profile;
        public static string steamAuthToken;
    }
}
