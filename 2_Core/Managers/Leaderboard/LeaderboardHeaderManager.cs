using System;
using BeatLeader.Manager;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    [UsedImplicitly]
    public class LeaderboardHeaderManager : IInitializable, ITickable, IDisposable {
        #region Initialize & Dispose

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
        private bool _initialized, _failed;

        private void LazyInit() {
            if (_initialized || _failed) return;

            try {
                var screenTransform = GameObject.Find("RightScreen").transform;
                var headerObject = screenTransform.FindChildRecursive("HeaderPanel").gameObject;

                _headerImage = headerObject.GetComponentInChildren<ImageView>();
                _boringColor0 = _headerImage.color0;
                _boringColor1 = _headerImage.color1;
                _initialized = true;
            } catch (Exception e) {
                Plugin.Log.Error($"LeaderboardHeaderManager initialization failed: {e.Message}");
                _failed = true;
            }
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
        }

        private void OnDisable() {
            if (!_initialized) return;
            _isFunny = false;
            _idle = false;
            _toleranceCheck = 0;
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