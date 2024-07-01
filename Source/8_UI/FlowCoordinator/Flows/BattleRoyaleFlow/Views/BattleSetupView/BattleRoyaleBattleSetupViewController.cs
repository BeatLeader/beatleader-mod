using BeatLeader.Components;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.UI.Hub {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.Flows.BattleRoyaleFlow.Views.BattleSetupView.BattleRoyaleBattleSetupView.bsml")]
    internal class BattleRoyaleBattleSetupViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region UI Components

        [UIValue("beatmap-filters-panel"), UsedImplicitly]
        private BeatmapFiltersPanel _beatmapFiltersPanel = null!;

        #endregion

        #region UI Values

        [UIValue("launch-button-interactable")]
        private bool LaunchButtonInteractable {
            get => _launchButtonInteractable;
            set {
                _launchButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _launchButtonInteractable;

        #endregion

        #region Setup

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            _beatmapFiltersPanel.DisplayToggles = false;
            _battleRoyaleHost.CanLaunchBattleStateChangedEvent += HandleBattleRoyaleCanLaunchStateChanged;
            _battleRoyaleHost.BattleLaunchStartedEvent += HandleBattleRoyaleLaunchStarted;
            _battleRoyaleHost.BattleLaunchFinishedEvent += HandleBattleRoyaleLaunchFinished;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.CanLaunchBattleStateChangedEvent -= HandleBattleRoyaleCanLaunchStateChanged;
            _battleRoyaleHost.BattleLaunchStartedEvent -= HandleBattleRoyaleLaunchStarted;
            _battleRoyaleHost.BattleLaunchFinishedEvent -= HandleBattleRoyaleLaunchFinished;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                _beatmapFiltersPanel = ReeUIComponentV2.Instantiate<BeatmapFiltersPanel>(transform);
                _battleRoyaleHost.FilterData = _beatmapFiltersPanel;
            }
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        #endregion

        #region Callbacks

        [UIAction("launch-battle-button-click"), UsedImplicitly]
        private void HandleLaunchButtonClicked() {
            _battleRoyaleHost.LaunchBattle();
        }
        
        private void HandleBattleRoyaleCanLaunchStateChanged(bool state) {
            LaunchButtonInteractable = state;
        }

        private void HandleBattleRoyaleLaunchStarted() {
            LaunchButtonInteractable = false;
        }

        private void HandleBattleRoyaleLaunchFinished() {
            LaunchButtonInteractable = true;
        }

        #endregion
    }
}