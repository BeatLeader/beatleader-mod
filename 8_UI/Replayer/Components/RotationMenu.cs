using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class RotationMenu : ReeUIComponentV2
    {
        public RotatingVRView View { get; private set; }

        [UIComponent("sync-button")]
        private BetterImage _syncButton;
        private Color _normalColor = Color.white;
        private Color _pressedColor = Color.cyan;
        private bool _syncEnabled;

        public event Action<bool> OnViewSyncChanged;

        public void Init(RotatingVRView view)
        {
            View = view;
        }
        public void EnableSync(bool enable)
        {
            if (View == null) return;
            View.followHead = enable;
            _syncButton.Image.color = enable ? _pressedColor : _normalColor;
            OnViewSyncChanged?.Invoke(enable);
        }

        protected override void OnInitialize()
        {
            var button = _syncButton.Image.gameObject.AddComponent<NoTransitionsButton>();
            button.onClick.AddListener(OnSyncButtonClicked);
            _syncButton.Image.color = _normalColor;
        }
        private void OnSyncButtonClicked()
        {
            EnableSync(_syncEnabled = !_syncEnabled);
        }
        [UIAction("rotate-left-button-click")] private void OnRotateLeftButtonClicked() => View?.Rotate(RotatingVRView.Direction.Left);
        [UIAction("rotate-center-button-click")] private void OnRotateCenterButtonClicked() => View?.RotateCenter();
        [UIAction("rotate-right-button-click")] private void OnRotateRightButtonClicked() => View?.Rotate(RotatingVRView.Direction.Right);
    }
}
