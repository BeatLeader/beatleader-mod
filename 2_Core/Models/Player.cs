namespace BeatLeader.Models
{
    internal class Player {
        public string id;
        public int rank;
        public string name;
        public string avatar;
        public string country;
        public int countryRank;
        public float pp;
        public string role;
        public Clan[] clans;
    }

    internal class Clan {
        public string tag;
        public string color;
    }
}
