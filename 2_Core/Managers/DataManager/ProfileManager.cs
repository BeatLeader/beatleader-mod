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

        public void Initialize() {
            LeaderboardEvents.UploadSuccessAction += UpdateProfile;
            UpdateProfile();
        }

        public void Dispose() {
            LeaderboardEvents.UploadSuccessAction -= UpdateProfile;
        }

        private async void UpdateProfile() {
            _profileRequestToken?.Cancel();
            _profileRequestToken = new CancellationTokenSource();

            LeaderboardEvents.ProfileRequestStarted();

            string userID = SteamUser.GetSteamID().m_SteamID.ToString();
            Player profile = await _httpUtils.GetData<Player>(String.Format(BLConstants.PROFILE_BY_ID, userID), _profileRequestToken.Token, null);
            if (profile == null) {
                Plugin.Log.Debug($"No profile for id {userID} was found. Abort");
                return;
            }

            LeaderboardEvents.PublishProfile(profile);
            BLContext.profile = profile;
        }
    }
}
