using BeatLeader.Utils;
using Steamworks;
using System;
using Zenject;

namespace BeatLeader.API
{
    class Authentication : IInitializable
    {
        private byte[]? m_Ticket;
        private uint m_pcbTicket;
        private HAuthTicket m_HAuthTicket;

        public void Initialize()
        {
            Login();
        }

        private void Login()
        {
            if (!SteamManager.Initialized)
            {
                Plugin.Log.Error("SteamManager is not initialized!");
                return;
            }

            Plugin.Log.Debug("Start of a steam auth process");
            m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);

            m_Ticket = new byte[1024];
            m_HAuthTicket = SteamUser.GetAuthSessionTicket(m_Ticket, 1024, out m_pcbTicket);
        }

        private void DropAuthData()
        {
            m_Ticket = null;
            m_pcbTicket = 0;
            m_HAuthTicket = HAuthTicket.Invalid;
        }

        protected Callback<GetAuthSessionTicketResponse_t>? m_GetAuthSessionTicketResponse;

        void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t response)
        {
            if (!m_HAuthTicket.Equals(response.m_hAuthTicket))
            {
                Plugin.Log.Debug("Unknown auth ticket");
                return;
            }

            Array.Resize(ref m_Ticket, (int)m_pcbTicket);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b in m_Ticket)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            var token = sb.ToString();

            Plugin.Log.Debug("Hex encoded ticket: " + token);

            if (m_HAuthTicket != HAuthTicket.Invalid)
            {
                var steamId = SteamUser.GetSteamID();

                EBeginAuthSessionResult beginAuthSessionResult = SteamUser.BeginAuthSession(m_Ticket, (int)m_pcbTicket, steamId);
                switch (beginAuthSessionResult)
                {
                    case EBeginAuthSessionResult.k_EBeginAuthSessionResultOK:
                        Plugin.Log.Error("Auth OK");
                        var result = SteamUser.UserHasLicenseForApp(steamId, new AppId_t(620980));

                        SteamUser.EndAuthSession(steamId);


                        switch (result)
                        {
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense:
                                Plugin.Log.Debug("License check: User does not have a license to current game");
                                DropAuthData();
                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense:

                                if (UploadManager.authToken == null)
                                {
                                    UploadManager.authToken = token;
                                }

                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultNoAuth:
                                Plugin.Log.Debug("License check: Unauthorized");
                                DropAuthData();
                                break;
                        }
                        break;
                    default:
                        Plugin.Log.Error("Auth check error: " + beginAuthSessionResult.ToString());
                        break;
                }
            }
        }
    }
}
