using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using Steamworks;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class ProfileManager : MonoBehaviour {
        private Coroutine _profileTask;

        private void Start() {
            LeaderboardEvents.UploadSuccessAction += UpdateFromScore;
            UpdateProfile();
        }

        private void OnDestroy() {
            LeaderboardEvents.UploadSuccessAction -= UpdateFromScore;
        }

        private void UpdateProfile() {
            if (_profileTask != null) {
                StopCoroutine(_profileTask);
            }

            LeaderboardEvents.ProfileRequestStarted();

            string userID = SteamUser.GetSteamID().m_SteamID.ToString();
            _profileTask = StartCoroutine(
                HttpUtils.GetData<Player>(String.Format(BLConstants.PROFILE_BY_ID, userID),
                profile => {
                    LeaderboardEvents.PublishProfile(profile);
                    BLContext.profile = profile;
                },
                () => {
                    Plugin.Log.Debug($"No profile for id {userID} was found. Abort");
                    LeaderboardEvents.NotifyProfileRequestFailed();
                    return;
                }));
        }

        private void UpdateFromScore(Score score) {
            if (score?.player != null) {
                LeaderboardEvents.PublishProfile(score.player);
                BLContext.profile = score.player;
            }
        }
    }
}
