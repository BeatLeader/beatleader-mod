using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
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

        public static bool IsCurrentPlayerTopClan(Clan clan) {
            return HasProfile && Profile.clans.Length > 0 && Profile.clans[0].id == clan.id;
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
            if (!_initialized) return;
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

        private static bool _initialized;
        
        public void Initialize() {
            UserRequest.StateChangedEvent += OnUserRequestStateChanged;
            UploadReplayRequest.StateChangedEvent += OnUploadRequestStateChanged;
            AddFriendRequest.StateChangedEvent += OnAddFriendRequestStateChanged;
            RemoveFriendRequest.StateChangedEvent += OnRemoveFriendRequestStateChanged;
            LeaderboardEvents.AddFriendWasPressedEvent += OnAddFriendWasPressed;
            LeaderboardEvents.RemoveFriendWasPressedEvent += OnRemoveFriendWasPressed;

            _initialized = true;
            UserRequest.Send();
        }

        public void Dispose() {
            _profileLoadTaskCompletionSource = null;
            _initialized = false;
            UserRequest.StateChangedEvent -= OnUserRequestStateChanged;
            UploadReplayRequest.StateChangedEvent -= OnUploadRequestStateChanged;
            AddFriendRequest.StateChangedEvent -= OnAddFriendRequestStateChanged;
            RemoveFriendRequest.StateChangedEvent -= OnRemoveFriendRequestStateChanged;
            LeaderboardEvents.AddFriendWasPressedEvent -= OnAddFriendWasPressed;
            LeaderboardEvents.RemoveFriendWasPressedEvent -= OnRemoveFriendWasPressed;
        }

        #endregion

        #region Events
        
        private void OnMainServerChanged(BeatLeaderServer value) {
            Authentication.ResetLogin();
            UserRequest.Send();
        }

        private static void OnAddFriendWasPressed(Player player) {
            AddFriendRequest.Send(player);
        }

        private static void OnRemoveFriendWasPressed(Player player) {
            RemoveFriendRequest.Send(player);
        }

        private static void OnAddFriendRequestStateChanged(WebRequests.IWebRequest<Player> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Failed:
                    LeaderboardEvents.ShowStatusMessage(failReason, LeaderboardEvents.StatusMessageType.Bad);
                    break;
                case WebRequests.RequestState.Finished:
                    AddFriend(instance.Result);
                    break;
            }
        }

        private static void OnRemoveFriendRequestStateChanged(WebRequests.IWebRequest<Player> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Failed:
                    LeaderboardEvents.ShowStatusMessage(failReason, LeaderboardEvents.StatusMessageType.Bad);
                    break;
                case WebRequests.RequestState.Finished:
                    RemoveFriend(instance.Result);
                    break;
            }
        }

        private static void OnUserRequestStateChanged(WebRequests.IWebRequest<User> instance, WebRequests.RequestState state, string? failReason) {
            if (state is WebRequests.RequestState.Failed) FinishTask();
            if (state is not WebRequests.RequestState.Finished) return;
            var result = instance.Result;
            Profile = result.player;
            Roles = FormatUtils.ParsePlayerRoles(result.player.role);
            SetFriends(result.friends);
            FinishTask();
        }

        private static void OnUploadRequestStateChanged(WebRequests.IWebRequest<Score> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished) return;
            Profile = instance.Result.Player;
        }

        #endregion
    }
}