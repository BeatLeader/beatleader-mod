using System;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Zenject;

using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.Manager;

namespace BeatLeader.DataManager
{
    internal class ProfileManager : IInitializable, IDisposable
    {
        private readonly HttpUtils _httpUtils;

        private CancellationTokenSource? _profileRequestToken;

        public ProfileManager(HttpUtils httpUtils)
        {
            _httpUtils = httpUtils;
        }

        public async void Initialize()
        {
            _profileRequestToken?.Cancel();
            _profileRequestToken = new CancellationTokenSource();

            LeaderboardEvents.ProfileRequestStarted();

            string userID = SteamUser.GetSteamID().m_SteamID.ToString();
            Profile profile = await loadProfile(userID, _profileRequestToken.Token);
            if (profile == null)
            {
                Plugin.Log.Debug($"No profile for id {userID} was found. Abort");
                return;
            }

            LeaderboardEvents.PublishProfile(profile);
            BLContext.profile = profile;
        }

        public void Dispose()
        {
        }

        private async Task<Profile> loadProfile(string userID, CancellationToken token)
        {
            return await _httpUtils.GetData<Profile>(String.Format(BLConstants.PROFILE_BY_ID, userID), token, null);
        }
    }
}
