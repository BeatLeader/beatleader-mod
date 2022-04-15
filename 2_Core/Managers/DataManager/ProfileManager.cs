using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using Steamworks;
using UnityEngine;
using Zenject;

namespace BeatLeader.DataManager {
    internal class ProfileManager : MonoBehaviour {
        private Coroutine _profileTask;

        private void Start() {
            LeaderboardEvents.UploadSuccessAction += UpdateProfile;
            UpdateProfile();
        }

        private void OnDestroy() {
            LeaderboardEvents.UploadSuccessAction -= UpdateProfile;
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
    }
}
