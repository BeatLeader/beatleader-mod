using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BeatLeader.API;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class ProfileManager : IInitializable, IDisposable {
        #region Roles

        public static PlayerRole[] Roles { get; private set; } = Array.Empty<PlayerRole>();

        #endregion

        #region Profile

        public static bool HasProfile { get; private set; }

        public static Player? Profile {
            get => _profile;
            private set {
                _profile = value;
                HasProfile = true;
            }
        }

        private static TaskCompletionSource<object?>? _profileLoadTaskCompletionSource;
        private static Player? _profile;

        public static bool IsCurrentPlayer(string otherId) {
            return HasProfile && string.Equals(Profile!.id, otherId, StringComparison.Ordinal);
        }

        public static bool IsCurrentPlayerInClan(Clan clan) {
            return HasProfile && Profile.clans.Any(profileClan => profileClan.id == clan.id);
        }

        public static bool TryGetUserId(out string? userId) {
            if (!HasProfile) {
                userId = null;
                return false;
            }

            userId = Profile!.id;
            return true;
        }
        
        public static Task WaitUntilProfileLoad() {
            AssignLoadProfileTaskIfNeeded();
            return _profileLoadTaskCompletionSource!.Task;
        }

        private static void AssignLoadProfileTaskIfNeeded() {
            _profileLoadTaskCompletionSource ??= new();
        }

        private static void FinishTask() {
            AssignLoadProfileTaskIfNeeded();
            _profileLoadTaskCompletionSource!.SetResult(null);
        }
        
        #endregion

        #region Friends

        public static event Action? FriendsUpdatedEvent;

        private static readonly Dictionary<string, Player> friends = new();

        private static void SetFriends(Player[] players) {
            friends.Clear();
            foreach (var player in players) {
                friends[player.id] = player;
            }

            FriendsUpdatedEvent?.Invoke();
        }

        private static void AddFriend(Player player) {
            friends[player.id] = player;
            FriendsUpdatedEvent?.Invoke();
        }

        private static void RemoveFriend(Player player) {
            friends.Remove(player.id);
            FriendsUpdatedEvent?.Invoke();
        }

        public static bool IsFriend(Player? player) {
            return player != null && friends.ContainsKey(player.id);
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
            _profileLoadTaskCompletionSource = null;
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
            if (state is API.RequestState.Failed) FinishTask();
            if (state is not API.RequestState.Finished) return;

            Profile = result.player;
            Roles = FormatUtils.ParsePlayerRoles(result.player.role);
            SetFriends(result.friends);
            FinishTask();
        }

        private static void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            if (state is not API.RequestState.Finished) return;
            Profile = result.Player;
        }

        #endregion
    }
}