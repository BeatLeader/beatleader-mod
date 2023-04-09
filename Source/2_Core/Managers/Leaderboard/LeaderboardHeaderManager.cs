using System;
using BeatLeader.Components;
using BeatLeader.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Zenject;
using Screen = HMUI.Screen;

namespace BeatLeader {
    [UsedImplicitly]
    public class LeaderboardHeaderManager : IInitializable, ITickable, IDisposable {
        #region Initialize & Dispose

        [Inject, UsedImplicitly]
        private LeaderboardView _leaderboardView;

        public void Initialize() {
            LeaderboardState.IsVisibleChangedEvent += OnVisibilityChanged;
        }

        public void Dispose() {
            LeaderboardState.IsVisibleChangedEvent -= OnVisibilityChanged;
        }

        #endregion

        #region LazyInit

        private static readonly Color _funnyColor0 = new Color(1.0f, 0.0f, 0.4f, 3.0f);
        private static readonly Color _funnyColor1 = new Color(0.3f, 0.0f, 1.0f, 3.0f);
        private Color _boringColor0;
        private Color _boringColor1;

        private ImageView _headerImage;
        private TextMeshProUGUI _headerText;
        private bool _initialized, _failed;
        private LeaderboardInfoPanel _infoPanel;

        private void LazyInit() {
            if (_initialized || _failed) return;

            try {
                if (!TryFindHeader(out var header)) return;

                _headerText = header.GetComponentInChildren<TextMeshProUGUI>();
                _headerImage = header.GetComponentInChildren<ImageView>();
                _infoPanel = ReeUIComponentV2.Instantiate<LeaderboardInfoPanel>(_headerImage.transform);
                _infoPanel.ManualInit(_headerImage.transform);
                _boringColor0 = _headerImage.color0;
                _boringColor1 = _headerImage.color1;
                _initialized = true;
            } catch (Exception e) {
                Plugin.Log.Error($"LeaderboardHeaderManager initialization failed: {e}");
                _failed = true;
            }
        }

        private bool TryFindHeader(out GameObject header) {
            var screen = _leaderboardView.gameObject.GetComponentInParent<Screen>();
            if (screen == null) {
                header = null;
                return false;
            }

            var leaderboardViewController = screen.transform.FindChildRecursive("PlatformLeaderboardViewController");
            if (leaderboardViewController == null) {
                header = null;
                return false;
            }

            header = leaderboardViewController.transform.FindChildRecursive("HeaderPanel").gameObject;
            return header != null;
        }

        #endregion

        #region Events

        private void OnVisibilityChanged(bool visible) {
            if (visible) {
                LazyInit();
                OnEnable();
            } else {
                OnDisable();
            }
        }

        private void OnEnable() {
            if (!_initialized) return;
            _isFunny = true;
            _idle = false;
            _toleranceCheck = 0;
            _headerText.enabled = false;
            _infoPanel.IsActive = true;
        }

        private void OnDisable() {
            if (!_initialized) return;
            _isFunny = false;
            _idle = false;
            _toleranceCheck = 0;
            _headerText.enabled = true;
            _infoPanel.IsActive = false;
        }

        #endregion

        #region Animation

        private const float Tolerance = 0.001f;
        private float _toleranceCheck;
        private bool _idle = true;
        private bool _isFunny;

        public void Tick() {
            if (_idle) return;

            Color target0, target1;

            if (_isFunny) {
                target0 = _funnyColor0;
                target1 = _funnyColor1;
            } else {
                target0 = _boringColor0;
                target1 = _boringColor1;
            }

            var t = Time.deltaTime * 10;
            _toleranceCheck = Mathf.Lerp(_toleranceCheck, 1, t);
            if (1 - _toleranceCheck < Tolerance) {
                ClampTo(target0, target1);
                _idle = true;
                return;
            }

            LerpTo(target0, target1, t);
        }

        private void LerpTo(Color color0, Color color1, float t) {
            _headerImage.color0 = Color.Lerp(_headerImage.color0, color0, t);
            _headerImage.color1 = Color.Lerp(_headerImage.color1, color1, t);
        }

        private void ClampTo(Color color0, Color color1) {
            _headerImage.color0 = color0;
            _headerImage.color1 = color1;
        }

        #endregion
    }
}