using BeatLeader.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.API
{
    class Authentication
    {
        public static void Login()
        {
            if (!SteamManager.Initialized)
            {
                Plugin.Log.Error($"SteamManager is not initialized!");
            }
            void OnAuthTicketResponse(GetAuthSessionTicketResponse_t response)
            {
                if (SteamHelper.Instance.lastTicket == response.m_hAuthTicket)
                {
                    SteamHelper.Instance.lastTicketResult = response.m_eResult;
                }
            };

            var steamId = SteamUser.GetSteamID();
            string authTicketHexString = "";

            byte[] authTicket = new byte[1024];
            var authTicketResult = SteamUser.GetAuthSessionTicket(authTicket, 1024, out var length);

            if (authTicketResult != HAuthTicket.Invalid)
            {
                Plugin.Log.Error("Auth ticket" + BitConverter.ToString(authTicket).Replace("-", ""));
                //var beginAuthSessionResult = SteamUser.BeginAuthSession(authTicket, (int)length, steamId);
                //switch (beginAuthSessionResult)
                //{
                //    case EBeginAuthSessionResult.k_EBeginAuthSessionResultOK:
                //        var result = SteamUser.UserHasLicenseForApp(steamId, new AppId_t(620980));

                //        SteamUser.EndAuthSession(steamId);

                //        switch (result)
                //        {
                //            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense:
                //                yield break;
                //            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense:
                //                if (SteamHelper.Instance.m_GetAuthSessionTicketResponse == null)
                //                    SteamHelper.Instance.m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthTicketResponse);


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
                //        break;
                //    default:
                //        yield break;
                //}
            }
        }
    }
}
