using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class ProfileManager : IInitializable, IDisposable {
        #region Roles

        public static event Action<PlayerRole[]> RolesUpdatedEvent;

        private static PlayerRole[] _roles = Array.Empty<PlayerRole>();

        public static PlayerRole[] Roles {
            get => _roles;
            private set {
                _roles = value;
                RolesUpdatedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Profile

        public static event Action<Player> ProfileUpdatedEvent;

        private static Player _profile;

        public static bool HasProfile;

        public static Player Profile {
            get => _profile;
            private set {
                _profile = value;
                HasProfile = true;
                ProfileUpdatedEvent?.Invoke(value);
            }
        }

        public static bool IsCurrentPlayer(string otherId) {
            return HasProfile && string.Equals(Profile.id, otherId, StringComparison.Ordinal);
        }

        public static bool IsCurrentPlayerInClan(Clan clan) {
            return HasProfile && Profile.clans.Any(profileClan => profileClan.id == clan.id);
        }

        public static bool IsCurrentPlayerTopClan(Clan clan) {
            return HasProfile && Profile.clans.Length > 0 && Profile.clans[0].id == clan.id;
        }

        public static bool TryGetUserId(out string userId) {
            if (!HasProfile) {
                userId = null;
                return false;
            }

            userId = Profile.id;
            return true;
        }

        #endregion

        #region Friends

        public static event Action FriendsUpdatedEvent;

        private static readonly Dictionary<string, Player> Friends = new();

        private static void SetFriends(Player[] players) {
            Friends.Clear();
            foreach (var player in players) {
                Friends[player.id] = player;
            }

            FriendsUpdatedEvent?.Invoke();
        }

        private static void AddFriend(Player player) {
            Friends[player.id] = player;
            FriendsUpdatedEvent?.Invoke();
        }

        private static void RemoveFriend(Player player) {
            Friends.Remove(player.id);
            FriendsUpdatedEvent?.Invoke();
        }

        public static bool IsFriend(Player player) {
            return player != null && Friends.ContainsKey(player.id);
        }

        #endregion

        #region Initialize / Dispose

        public void Initialize() {
            UserRequest.AddStateListener(OnUserRequestStateChanged);
            UploadReplayRequest.AddStateListener(OnUploadRequestStateChanged);
            AddFriendRequest.AddStateListener(OnAddFriendRequestStateChanged);
            RemoveFriendRequest.AddStateListener(OnRemoveFriendRequestStateChanged);
            LeaderboardEvents.AddFriendWasPressedEvent += OnAddFriendWasPressed;
            LeaderboardEvents.RemoveFriendWasPressedEvent += OnRemoveFriendWasPressed;
            PluginConfig.MainServerChangedEvent += OnMainServerChanged;

            UserRequest.SendRequest();
        }

        public void Dispose() {
            UserRequest.RemoveStateListener(OnUserRequestStateChanged);
            UploadReplayRequest.RemoveStateListener(OnUploadRequestStateChanged);
            AddFriendRequest.RemoveStateListener(OnAddFriendRequestStateChanged);
            RemoveFriendRequest.RemoveStateListener(OnRemoveFriendRequestStateChanged);
            LeaderboardEvents.AddFriendWasPressedEvent -= OnAddFriendWasPressed;
            LeaderboardEvents.RemoveFriendWasPressedEvent -= OnRemoveFriendWasPressed;
            PluginConfig.MainServerChangedEvent -= OnMainServerChanged;
        }

        #endregion

        #region Events
        
        private void OnMainServerChanged(BeatLeaderServer value) {
            Authentication.ResetLogin();
            UserRequest.SendRequest();
        }

        private static void OnAddFriendWasPressed(Player player) {
            AddFriendRequest.SendRequest(player);
        }

        private static void OnRemoveFriendWasPressed(Player player) {
            RemoveFriendRequest.SendRequest(player);
        }

        private static void OnAddFriendRequestStateChanged(API.RequestState state, Player result, string failReason) {
            switch (state) {
                case API.RequestState.Failed:
                    LeaderboardEvents.ShowStatusMessage(failReason, LeaderboardEvents.StatusMessageType.Bad);
                    break;
                case API.RequestState.Finished:
                    AddFriend(result);
                    break;
            }
        }

        private static void OnRemoveFriendRequestStateChanged(API.RequestState state, Player result, string failReason) {
            switch (state) {
                case API.RequestState.Failed:
                    LeaderboardEvents.ShowStatusMessage(failReason, LeaderboardEvents.StatusMessageType.Bad);
                    break;
                case API.RequestState.Finished:
                    RemoveFriend(result);
                    break;
            }
        }

        private static void OnUserRequestStateChanged(API.RequestState state, User result, string failReason) {
            if (state is not API.RequestState.Finished) return;

            Profile = result.player;
            Roles = FormatUtils.ParsePlayerRoles(result.player.role);
            SetFriends(result.friends);
        }

        private static void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            if (state is not API.RequestState.Finished) return;
            Profile = result.Player;
        }

        #endregion
    }
}