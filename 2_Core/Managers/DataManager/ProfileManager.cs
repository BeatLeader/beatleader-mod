using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using Steamworks;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class ProfileManager : MonoBehaviour {
        private Coroutine _profileTask;

        private void Start() {
            LeaderboardState.UploadRequest.FinishedEvent += UpdateFromScore;
            UpdateProfile();
        }

        private void OnDestroy() {
            LeaderboardState.UploadRequest.FinishedEvent -= UpdateFromScore;
        }

        private void UpdateProfile() {
            if (_profileTask != null) {
                StopCoroutine(_profileTask);
                LeaderboardState.ProfileRequest.TryNotifyCancelled();
            }

            LeaderboardState.ProfileRequest.NotifyStarted();

            string userID = SteamUser.GetSteamID().m_SteamID.ToString();
            _profileTask = StartCoroutine(
                HttpUtils.GetData<Player>(String.Format(BLConstants.PROFILE_BY_ID, userID),
                profile => {
                    LeaderboardState.ProfileRequest.NotifyFinished(profile);
                    BLContext.profile = profile;
                },
                reason => {
                    Plugin.Log.Debug($"No profile for id {userID} was found. Abort");
                    LeaderboardState.ProfileRequest.NotifyFailed(reason);
                }));
        }

        private void UpdateFromScore(Score score) {
            if (score?.player != null) {
                LeaderboardState.ProfileRequest.NotifyFinished(score.player);
                BLContext.profile = score.player;
            }
        }
    }
}
