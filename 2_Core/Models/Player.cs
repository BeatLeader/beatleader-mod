namespace BeatLeader.Models
{
     public class Player {
        public string id;
        public int rank;
        public string name;
        public string avatar;
        public string country;
        public int countryRank;
        public float pp;
        public string role;
        public Clan[] clans;
        public PatreonFeatures patreonFeatures;
    }

    public class Clan {
        public string tag;
        public string color;
    }

    public class PatreonFeatures {
        public string message;
    }
}
