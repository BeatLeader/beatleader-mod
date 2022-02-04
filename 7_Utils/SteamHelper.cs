using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    public class SteamHelper
    {
        private static SteamHelper _instance;
        public static SteamHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SteamHelper();
                return _instance;
            }
        }
        public Steamworks.HAuthTicket lastTicket;
        public Steamworks.EResult lastTicketResult;

        public Steamworks.Callback<Steamworks.GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;
    }
}
