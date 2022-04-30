using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BeatLeader.Components {
    internal class Logo : ReeUIComponentV2 {
        #region Animation

        private static readonly int GlowPropertyId = Shader.PropertyToID("_Glow");
        private static readonly int DotScalePropertyId = Shader.PropertyToID("_DotScale");
        private static readonly int BlockScalePropertyId = Shader.PropertyToID("_BlockScale");
        private static readonly int CornerRadiusPropertyId = Shader.PropertyToID("_CornerRadius");
        private static readonly int ThicknessPropertyId = Shader.PropertyToID("_Thickness");
        private static readonly int SpinnerRotationPropertyId = Shader.PropertyToID("_SpinnerRotation");
        private static readonly int SpinnerAmplitudePropertyId = Shader.PropertyToID("_SpinnerAmplitude");

        private const float IdleGlow = 0.15f;
        private const float IdleDotScale = 0.25f;
        private const float IdleBlockScale = 0.7f;
        private const float IdleCornerRadius = 0.2f;
        private const float IdleThickness = 0.08f;
        private const float IdleFill = 1.0f;

        private const float ThinkingGlow = 0.9f;
        private const float ThinkingDotScale = 0.1f;
        private const float ThinkingBlockScale = 0.5f;
        private const float ThinkingCornerRadius = 0.5f;
        private const float ThinkingThickness = 0.16f;
        private const float ThinkingFill = 0.4f;
        private const float ThinkingRotationSpeed = 12.0f;

        private float _glow = IdleGlow;
        private float _dotScale = IdleDotScale;
        private float _blockScale = IdleBlockScale;
        private float _cornerRadius = IdleCornerRadius;
        private float _thickness = IdleThickness;
        private float _fill = IdleFill;
        private float _targetSpinnerRotation;
        private float _spinnerRotation;

        private bool _isThinking;
        private const float HalfPI = Mathf.PI / 2;

        private bool Thinking {
            get => _isThinking;
            set {
                if (_isThinking.Equals(value)) return;
                _isThinking = value;
                if (value) return;
                _targetSpinnerRotation = Mathf.CeilToInt(_spinnerRotation / HalfPI) * HalfPI;
            }
        }

        private void Update() {
            var deltaTime = Time.deltaTime;
            var slowT = deltaTime * 10f;
            var fastT = deltaTime * 20f;

            if (Thinking) {
                _glow = Mathf.Lerp(_glow, ThinkingGlow, fastT);
                _dotScale = Mathf.Lerp(_dotScale, ThinkingDotScale, slowT);
                _blockScale = Mathf.Lerp(_blockScale, ThinkingBlockScale, slowT);
                _cornerRadius = Mathf.Lerp(_cornerRadius, ThinkingCornerRadius, slowT);
                _thickness = Mathf.Lerp(_thickness, ThinkingThickness, slowT);
                _fill = Mathf.Lerp(_fill, ThinkingFill, slowT);
                _spinnerRotation += ThinkingRotationSpeed * deltaTime;
            } else {
                _glow = Mathf.Lerp(_glow, IdleGlow, fastT);
                _dotScale = Mathf.Lerp(_dotScale, IdleDotScale, slowT);
                _blockScale = Mathf.Lerp(_blockScale, IdleBlockScale, slowT);
                _cornerRadius = Mathf.Lerp(_cornerRadius, IdleCornerRadius, slowT);
                _thickness = Mathf.Lerp(_thickness, IdleThickness, slowT);
                _fill = Mathf.Lerp(_fill, IdleFill, fastT);
                _spinnerRotation = Mathf.Lerp(_spinnerRotation, _targetSpinnerRotation, fastT);
            }

            SetMaterialProperties();
        }

        private void SetMaterialProperties() {
            _materialInstance.SetFloat(GlowPropertyId, _glow);
            _materialInstance.SetFloat(DotScalePropertyId, _dotScale);
            _materialInstance.SetFloat(BlockScalePropertyId, _blockScale);
            _materialInstance.SetFloat(CornerRadiusPropertyId, _cornerRadius);
            _materialInstance.SetFloat(ThicknessPropertyId, _thickness);
            _materialInstance.SetFloat(SpinnerAmplitudePropertyId, _fill);
            _materialInstance.SetFloat(SpinnerRotationPropertyId, _spinnerRotation);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnAfterParse() {
            SetMaterial();
            
            LeaderboardState.ProfileRequest.StateChangedEvent += OnProfileRequestStateChanged;
            LeaderboardState.ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            LeaderboardState.UploadRequest.StateChangedEvent += OnUploadRequestStateChanged;
            
            OnProfileRequestStateChanged(LeaderboardState.ProfileRequest.State);
            OnScoresRequestStateChanged(LeaderboardState.ScoresRequest.State);
            OnUploadRequestStateChanged(LeaderboardState.UploadRequest.State);
        }

        protected override void OnDispose() {
            LeaderboardState.ProfileRequest.StateChangedEvent -= OnProfileRequestStateChanged;
            LeaderboardState.ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
            LeaderboardState.UploadRequest.StateChangedEvent -= OnUploadRequestStateChanged;
        }

        #endregion

        #region Events

        private void OnProfileRequestStateChanged(RequestState requestState) {
            _loadingProfile = requestState switch {
                RequestState.Uninitialized => false,
                RequestState.Started => true,
                RequestState.Failed => false,
                RequestState.Finished => false,
                _ => throw new ArgumentOutOfRangeException(nameof(requestState), requestState, null)
            };
            UpdateState();
        }

        private void OnScoresRequestStateChanged(RequestState requestState) {
            _loadingScores = requestState switch {
                RequestState.Uninitialized => false,
                RequestState.Started => true,
                RequestState.Failed => false,
                RequestState.Finished => false,
                _ => throw new ArgumentOutOfRangeException(nameof(requestState), requestState, null)
            };
            UpdateState();
        }

        private void OnUploadRequestStateChanged(RequestState requestState) {
            _uploadingScore = requestState switch {
                RequestState.Uninitialized => false,
                RequestState.Started => true,
                RequestState.Failed => false,
                RequestState.Finished => false,
                _ => throw new ArgumentOutOfRangeException(nameof(requestState), requestState, null)
            };
            UpdateState();
        }

        #endregion

        #region UpdateState

        private bool _uploadingScore;
        private bool _loadingProfile;
        private bool _loadingScores;

        private void UpdateState() {
            Thinking = _loadingProfile || _loadingScores || _uploadingScore;
        }

        #endregion

        #region Image & Material

        [UIComponent("logo-image"), UsedImplicitly]
        private Image _logoImage;

        private Material _materialInstance;

        private void SetMaterial() {
            _materialInstance = Object.Instantiate(BundleLoader.LogoMaterial);
            _logoImage.material = _materialInstance;
        }

        #endregion
    }
}