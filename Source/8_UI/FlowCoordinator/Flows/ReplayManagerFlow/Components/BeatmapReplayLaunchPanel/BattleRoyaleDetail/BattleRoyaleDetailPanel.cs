using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleDetailPanel : ReeUIComponentV3<BattleRoyaleDetailPanel>, BeatmapReplayLaunchPanel.IDetailPanel {
        #region UI Components

        [UIValue("replay-info-panel"), UsedImplicitly]
        private ReplayStatisticsPanel _replayStatisticsPanel = null!;

        [UIComponent("mini-profile"), UsedImplicitly]
        private QuickMiniProfile _miniProfile = null!;

        #endregion

        #region UI Values

        [UIValue("select-button-text"), UsedImplicitly]
        private string SelectButtonText {
            get => _selectButtonText;
            set {
                _selectButtonText = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("select-button-interactable"), UsedImplicitly]
        private bool SelectButtonInteractable {
            get => _selectButtonInteractable;
            set {
                _selectButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        private string _selectButtonText = null!;
        private bool _selectButtonInteractable;

        #endregion

        #region DetailPanel
        
        private IBeatmapReplayLaunchPanel? _beatmapReplayLaunchPanel;
        private IReplayHeader? _header;

        public void Setup(IBeatmapReplayLaunchPanel? launchPanel, Transform? parent) {
            _beatmapReplayLaunchPanel = launchPanel;
            UpdateSelectButton();
            ContentTransform.SetParent(parent, false);
            Content.SetActive(parent is not null);
        }

        public void SetData(IReplayHeader? header) {
            _header = header;
            UpdateSelectButton();
            _replayStatisticsPanel.SetDataByHeaderAsync(header);
            LoadPlayerAsync(header);
        }

        private async void LoadPlayerAsync(IReplayHeaderBase? header) {
            var player = header is null ? null : await header.LoadPlayerAsync(false, default);
            _miniProfile.SetPlayer(player);
        }

        #endregion

        #region Setup

        protected override void OnInstantiate() {
            _replayStatisticsPanel = ReeUIComponentV2.Instantiate<ReplayStatisticsPanel>(transform);
        }

        protected override bool OnValidation() {
            return _beatmapReplayLaunchPanel is not null;
        }

        #endregion

        #region SelectButton

        private bool _replayIsAdded;

        private void UpdateSelectButton() {
            ValidateAndThrow();
            var interactable = _header is not null;
            SelectButtonInteractable = interactable;
            var containsCurrent = !interactable || _beatmapReplayLaunchPanel!.SelectedReplays.Contains(_header!);
            SelectButtonText = containsCurrent ? "Remove" : "Select";
            _replayIsAdded = containsCurrent;
        }

        #endregion

        #region Callbacks

        [UIAction("select-button-click"), UsedImplicitly]
        private void HandleSelectButtonClicked() {
            ValidateAndThrow();
            if (_replayIsAdded) {
                _beatmapReplayLaunchPanel!.RemoveSelectedReplay(_header!);
            } else {
                _beatmapReplayLaunchPanel!.AddSelectedReplay(_header!);
            }
            UpdateSelectButton();
        }

        #endregion
    }
}