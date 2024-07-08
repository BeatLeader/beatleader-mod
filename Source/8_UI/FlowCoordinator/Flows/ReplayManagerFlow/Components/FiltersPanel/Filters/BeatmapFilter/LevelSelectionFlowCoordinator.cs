using System;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class LevelSelectionFlowCoordinator : FlowCoordinator {
        #region Injection

        [Inject] private readonly LevelSelectionViewController _levelSelectionViewController = null!;

        #endregion

        #region Setup

        public bool AllowDifficultySelection {
            get => _levelSelectionViewController.allowDifficultySelection;
            set => _levelSelectionViewController.allowDifficultySelection = value;
        }

        public event Action<IDifficultyBeatmap>? BeatmapSelectedEvent;
        public event Action? FlowCoordinatorDismissedEvent;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            _levelSelectionViewController.BeatmapSelectedEvent += HandleBeatmapSelected;
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("SELECT LEVEL");
            ProvideInitialViewControllers(_levelSelectionViewController);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            //to avoid changes while not in this menu
            _levelSelectionViewController.BeatmapSelectedEvent -= HandleBeatmapSelected;
        }

        private void Dismiss() {
            this.DismissSelf();
            FlowCoordinatorDismissedEvent?.Invoke();
        }

        #endregion

        #region Callbacks

        protected override void BackButtonWasPressed(ViewController viewController) {
            Dismiss();
        }

        private void HandleBeatmapSelected(IDifficultyBeatmap beatmap) {
            BeatmapSelectedEvent?.Invoke(beatmap);
            Dismiss();
        }

        #endregion
    }
}