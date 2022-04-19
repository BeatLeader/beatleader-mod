using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.TopPanel.Logo.bsml")]
    internal class Logo : ReeUIComponent {
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
        private const float IdleRotationSpeed = 0.0f;

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
        private float _rotationSpeed = IdleRotationSpeed;
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

        protected override void OnInitialize() {
            LeaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
            LeaderboardEvents.ProfileRequestFailedEvent += OnProfileRequestFailed;

            LeaderboardEvents.ScoresRequestStartedEvent += OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresRequestFinished;
            LeaderboardEvents.ScoresFetchFailedEvent += OnScoresRequestFailed;

            LeaderboardEvents.UploadStartedAction += OnUploadStarted;
            LeaderboardEvents.UploadSuccessAction += OnUploadSuccess;
            LeaderboardEvents.UploadFailedAction += OnUploadFailed;

            SetMaterial();
        }

        protected override void OnDispose() {
            LeaderboardEvents.UserProfileStartedEvent -= OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent -= OnProfileFetched;
            LeaderboardEvents.ProfileRequestFailedEvent -= OnProfileRequestFailed;

            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresRequestFinished;
            LeaderboardEvents.ScoresFetchFailedEvent -= OnScoresRequestFailed;

            LeaderboardEvents.UploadStartedAction -= OnUploadStarted;
            LeaderboardEvents.UploadSuccessAction -= OnUploadSuccess;
            LeaderboardEvents.UploadFailedAction -= OnUploadFailed;
        }

        #endregion

        #region Events

        private void OnScoresRequestStarted() {
            _loadingScores = true;
            UpdateState();
        }

        private void OnScoresRequestFinished(Paged<Score> paged) {
            _loadingScores = false;
            UpdateState();
        }

        private void OnScoresRequestFailed() {
            _loadingScores = false;
            UpdateState();
        }

        private void OnProfileRequestStarted() {
            _loadingProfile = true;
            UpdateState();
        }

        private void OnProfileFetched(Player p) {
            _loadingProfile = false;
            UpdateState();
        }

        private void OnProfileRequestFailed() {
            _loadingProfile = false;
            UpdateState();
        }

        private void OnUploadStarted() {
            _uploadingScore = true;
            UpdateState();
        }

        private void OnUploadSuccess(Score score) {
            _uploadingScore = false;
            UpdateState();
        }

        private void OnUploadFailed(bool completely, int retry) {
            _uploadingScore = !completely;
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