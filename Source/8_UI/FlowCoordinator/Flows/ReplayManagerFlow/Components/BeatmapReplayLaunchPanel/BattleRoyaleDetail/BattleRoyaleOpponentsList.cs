using System.Reflection;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class BattleRoyaleOpponentsList : ReeListComponentBase<BattleRoyaleOpponentsList, IReplayHeaderBase, BattleRoyaleOpponentsList.Cell> {
        #region Cells

        public class Cell : ReeTableCell<Cell, IReplayHeaderBase> {
            #region UI Components

            [UIValue("player-avatar"), UsedImplicitly]
            private PlayerAvatar _playerAvatar = null!;

            [UIComponent("player-name"), UsedImplicitly]
            private TMP_Text _playerNameText = null!;

            [UIComponent("date"), UsedImplicitly]
            private TMP_Text _dateText = null!;

            [UIObject("background"), UsedImplicitly]
            private GameObject _backgroundObject = null!;

            #endregion

            #region Setup

            protected override string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(
                "BattleRoyaleOpponentsListCell", Assembly.GetExecutingAssembly()
            );

            private IBattleRoyaleHost _battleRoyaleHost = null!;

            protected override void Init(IReplayHeaderBase item) {
                RefreshPlayer();
            }

            public void Init(IBattleRoyaleHost battleRoyaleHost) {
                _battleRoyaleHost = battleRoyaleHost;
            }

            protected override void OnInstantiate() {
                _playerAvatar = ReeUIComponentV2.Instantiate<PlayerAvatar>(transform);
                var root = _playerAvatar.GetRootTransform().gameObject;
                var layoutElement = root.GetComponent<LayoutElement>();
                layoutElement.preferredWidth = 10;
            }

            protected override void OnInitialize() {
                var background = _backgroundObject.AddComponent<AdvancedImageView>();
                background.sprite = GameResources.RoundRectSprite;
                background.type = Image.Type.Sliced;
                background.gradient = true;
                background.material = GameResources.UINoGlowAdditiveMaterial;
                background.color1 = ColorUtils.RandomColor();
                background.SetField<ImageView, float>("_skew", 0.08f);
                background.__Refresh();
            }

            #endregion

            #region LoadPlayer

            private async void RefreshPlayer() {
                var player = await Item!.LoadPlayerAsync(false, default);
                _playerAvatar.SetPlayer(player);
                _playerNameText.text = player.Name;
            }

            #endregion

            #region Callbacks

            [UIAction("navigate-button-click"), UsedImplicitly]
            private void HandleNavigateButtonClicked() {
                _battleRoyaleHost.NavigateTo(Item!);
            }

            [UIAction("remove-button-click"), UsedImplicitly]
            private void HandleRemoveButtonClicked() {
                _battleRoyaleHost.RemoveReplay(Item!, this);
            }

            #endregion
        }

        #endregion

        #region Setup

        protected override float CellSize => 12;

        private IBattleRoyaleHost? _battleRoyaleHost;

        public void Setup(IBattleRoyaleHost battleRoyaleHost) {
            if (_battleRoyaleHost is not null) {
                _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
                _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
            }
            _battleRoyaleHost = battleRoyaleHost;
            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
        }

        protected override void OnCellConstruct(Cell cell) {
            ValidateAndThrow();
            cell.Init(_battleRoyaleHost!);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            CellSelectionType = TableViewSelectionType.None;
        }

        protected override bool OnValidation() {
            return _battleRoyaleHost is not null;
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IReplayHeaderBase header, object caller) {
            items.Add(header);
            Refresh();
        }

        private void HandleReplayRemoved(IReplayHeaderBase header, object caller) {
            items.Remove(header);
            Refresh();
        }

        #endregion
    }
}