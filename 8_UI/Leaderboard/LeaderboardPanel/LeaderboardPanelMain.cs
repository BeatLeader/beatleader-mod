using System;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal partial class LeaderboardPanel : BSMLAutomaticViewController, IInitializable, IDisposable {
        #region Inject

        [Inject] [UsedImplicitly] private LeaderboardEvents _leaderboardEvents;

        #endregion

        #region Init/Dispose

        public void Initialize() {
            InitializeProfileSection();
        }

        public void Dispose() {
            DisposeProfileSection();
        }

        #endregion
    }
}