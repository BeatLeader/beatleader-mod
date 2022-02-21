using BeatLeader.Utils;
using Steamworks;
using System;
using Zenject;

namespace BeatLeader.API
{
    class Authentication : IInitializable
    {
        public void Initialize()
        {
            Login();
        }

        private void Login()
        {
            if (!SteamManager.Initialized)
            {
                Plugin.Log.Error($"SteamManager is not initialized!");
            }
            void OnAuthTicketResponse(GetAuthSessionTicketResponse_t response)
            {
                if (SteamHelper.Instance.lastTicket == response.m_hAuthTicket)
                {

                }

                SteamHelper.Instance.lastTicketResult = response.m_eResult;
                byte[] authTicket = new byte[1024];

                var authTicketResult = SteamUser.GetAuthSessionTicket(authTicket, 1024, out var length);
                Array.Resize(ref authTicket, (int)length);
                var authticketStr = BitConverter.ToString(authTicket).Replace("-", "");
                Plugin.Log.Error("Auth ticket 2: " + authticketStr);
            };

            var steamId = SteamUser.GetSteamID();
            string authTicketHexString = "";

            byte[] authTicket = new byte[1024];
            var authTicketResult = SteamUser.GetAuthSessionTicket(authTicket, 1024, out var length);

            if (authTicketResult != HAuthTicket.Invalid)
            {
                Array.Resize(ref authTicket, (int)length);

                var beginAuthSessionResult = SteamUser.BeginAuthSession(authTicket, (int)length, steamId);
                Plugin.Log.Error("Auth ticket 1" + BitConverter.ToString(authTicket).Replace("-", ""));
                if (UploadManager.authToken == null) UploadManager.authToken = BitConverter.ToString(authTicket).Replace("-", "");
                switch (beginAuthSessionResult)
                {
                    case EBeginAuthSessionResult.k_EBeginAuthSessionResultOK:
                        Plugin.Log.Error("Auth OK");
                        var result = SteamUser.UserHasLicenseForApp(steamId, new AppId_t(620980));

                        SteamUser.EndAuthSession(steamId);


                        //        switch (result)
                        //        {
                        //            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense:
                        //                yield break;
                        //            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense:
                        //                if (SteamHelper.Instance.m_GetAuthSessionTicketResponse == null)
                                          //SteamHelper.Instance.m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthTicketResponse);


                        //                SteamHelper.Instance.lastTicket = SteamUser.GetAuthSessionTicket(authTicket, 1024, out length);
                        //                if (SteamHelper.Instance.lastTicket != HAuthTicket.Invalid)
                        //                {
                        //                    Array.Resize(ref authTicket, (int)length);
                        //                    authTicketHexString = BitConverter.ToString(authTicket).Replace("-", "");
                        //                }

                        //                break;
                        //            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultNoAuth:
                        //                yield break;
                        //        }
                        break;
                        //    default:
                        //        yield break;
                }
            }
        }
    }
}
