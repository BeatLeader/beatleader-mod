using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AnchorSelector : ReeUIComponentV2 {
        #region Config

        private static readonly Vector2[] anchorPoints = new Vector2[] {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1)
        };

        #endregion

        #region UI Components

        [UIComponent("container")]
        private readonly RectTransform _containerRect;

        [UIComponent("background")]
        private readonly BetterImage _backgroundImage;

        private CanvasGroup _backgroundGroup;

        #endregion

        #region Setup

        public void Select(Vector2 anchor) {
            SetEnabled(true);
            Generate();
            foreach (var button in _anchorButtons) {
                if (button.anchor == anchor) {
                    button.Select();
                    break;
                }
            }
        }

        public void SetEnabled(bool state) {
            _backgroundGroup.alpha = state ? 1 : 0.8f;
        }

        protected override void OnInitialize() {
            _backgroundGroup = _backgroundImage.gameObject.AddComponent<CanvasGroup>();
            SetEnabled(false);
        }

        #endregion

        #region Events

        public event Action<Vector2> AnchorSelectedEvent;

        #endregion

        #region Generator

        private readonly Sprite _selectorSprite = BundleLoader.AnchorBGDots;
        private readonly List<AnchorButton> _anchorButtons = new();
        private AnchorButton _selectedAnchor;

        public void Generate() {
            if (_anchorButtons.Count != 0) return;
            foreach (var point in anchorPoints) {
                var button = new GameObject($"{point.x}{point.y}").AddComponent<AnchorButton>();
                button.transform.SetParent(_containerRect, false);
                button.Setup(_selectorSprite, point, _containerRect.rect.size / 2);
                button.AnchorSelectedEvent += HandleAnchorSelected;
                _anchorButtons.Add(button);
            }
        }

        public void Degenerate() {
            foreach (var button in _anchorButtons) {
                button.AnchorSelectedEvent -= HandleAnchorSelected;
                button.gameObject.TryDestroy();
            }
            _anchorButtons.Clear();
        }

        public void RebuildImmediate() {
            foreach (var button in _anchorButtons) {
                button.sprite = _selectorSprite;
                button.size = _containerRect.rect.size / 2;
                button.Refresh();
            }
        }

        #endregion

        #region Callbacks

        private void HandleAnchorSelected(AnchorButton button) {
            _selectedAnchor?.RemoveSelection();
            _selectedAnchor = button;
            AnchorSelectedEvent?.Invoke(button.anchor);
        }

        #endregion
    }
}
