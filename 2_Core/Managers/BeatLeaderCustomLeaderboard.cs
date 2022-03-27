using System;
using BeatLeader.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using Zenject;

namespace BeatLeader {
    [UsedImplicitly]
    internal class BeatLeaderCustomLeaderboard : CustomLeaderboard, IInitializable, IDisposable {
        #region Inject

        private readonly CustomLeaderboardManager _customLeaderboardManager;
        private readonly LeaderboardPanel _leaderboardPanel;
        private readonly LeaderboardView _leaderboardView;

        public BeatLeaderCustomLeaderboard(
            CustomLeaderboardManager customLeaderboardManager,
            LeaderboardPanel panelViewController,
            LeaderboardView leaderboardViewController
        ) {
            _customLeaderboardManager = customLeaderboardManager;
            _leaderboardPanel = panelViewController;
            _leaderboardView = leaderboardViewController;
        }

        #endregion

        #region CustomLeaderboard Implementation

        protected override ViewController panelViewController => _leaderboardPanel;
        protected override ViewController leaderboardViewController => _leaderboardView;

        #endregion

        #region Initialize & Dispose   (Register/UnRegister)

        public void Initialize() {
            _customLeaderboardManager.Register(this);
        }

        public void Dispose() {
            _customLeaderboardManager.Unregister(this);
        }

        #endregion
    }
}