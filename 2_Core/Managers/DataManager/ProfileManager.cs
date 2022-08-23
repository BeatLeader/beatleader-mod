using System;
using System.Collections.Generic;
using BeatLeader.API.Methods;
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

        public static bool IsCurrentPlayer(Player other) {
            return HasProfile && string.Equals(Profile.id, other?.id, StringComparison.Ordinal);
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

        public static void AddFriend(Player player) {
            Friends[player.id] = player;
            FriendsUpdatedEvent?.Invoke();
        }

        public static void RemoveFriend(Player player) {
            Friends.Remove(player.id);
            FriendsUpdatedEvent?.Invoke();
        }

        public static bool IsFriend(Player player) {
            return Friends.ContainsKey(player.id);
        }

        #endregion

        #region Initialize / Dispose

        public void Initialize() {
            UserRequest.AddStateListener(OnUserRequestStateChanged);
            LeaderboardState.UploadRequest.FinishedEvent += OnUploadFinished;

            UserRequest.SendRequest();
        }

        public void Dispose() {
            UserRequest.RemoveStateListener(OnUserRequestStateChanged);
            LeaderboardState.UploadRequest.FinishedEvent -= OnUploadFinished;
        }

        #endregion

        #region Events

        private static void OnUserRequestStateChanged(API.RequestState state, User result, string failReason) {
            if (state is not API.RequestState.Finished) return;

            Profile = result.player;
            Roles = FormatUtils.ParsePlayerRoles(result.player.role);
            SetFriends(result.friends);
        }

        private static void OnUploadFinished(Score score) {
            Profile = score.player;
        }

        #endregion
    }
}