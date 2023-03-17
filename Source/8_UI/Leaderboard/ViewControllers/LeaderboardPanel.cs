using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController {
        #region PreParser

        [Inject, UsedImplicitly]
        private PreParser _preParser;

        public class PreParser : MonoBehaviour {
            public StatusBar statusBar;
            public Logo logo;
            public PlayerInfo playerInfo;

            private void Awake() {
                statusBar = ReeUIComponentV2.InstantiateOnSceneRoot<StatusBar>();
                logo = ReeUIComponentV2.InstantiateOnSceneRoot<Logo>();
                playerInfo = ReeUIComponentV2.InstantiateOnSceneRoot<PlayerInfo>();
            }
        }

        #endregion

        #region Components

        [UIValue("status-bar"), UsedImplicitly]
        private StatusBar StatusBar => _preParser.statusBar;

        [UIValue("logo"), UsedImplicitly]
        private Logo Logo => _preParser.logo;

        [UIValue("player-info"), UsedImplicitly]
        private PlayerInfo PlayerInfo => _preParser.playerInfo;

        private void Awake() {
            StatusBar.SetParent(transform);
            Logo.SetParent(transform);
            PlayerInfo.SetParent(transform);
        }

        #endregion
    }
}