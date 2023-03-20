namespace BeatLeader.Models
{
    public class User {
        public Player player;
        public Player[] friends;
    }
    
    public class Player {
        public string id;
        public int rank;
        public string name;
        public string? avatar;
        public string country;
        public int countryRank;
        public float pp;
        public string role;
        public Clan[] clans;
        public ServiceIntegration[] socials;
        public ProfileSettings profileSettings;
    }

    public class Clan {
        public string tag;
        public string color;
    }

    public class ProfileSettings {
        public string message;
        public string effectName;
        public int hue;
        public float saturation;
    }

    public class ServiceIntegration {
        public string service;
        public string link;
        public string user;
    }
}
