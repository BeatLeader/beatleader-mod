using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace BeatLeader {
    internal abstract class BattleRoyaleVRController : VRController {
        #region Injection
        [Inject]
        TimeHelper timeHelper;

        #endregion

        #region Unity Events

        private void Awake() {
            gameObject.SetActive(false);
            _propertyBlock = new MaterialPropertyBlock();
            _saberTrail = gameObject.AddComponent<SaberTrail>();
            _saberTrail._trailRendererPrefab = InstantiateTrailRenderer();
            _saberTrail._trailDuration = 0.4f;
            _saberTrail._whiteSectionMaxDuration = 0.001f;
            _saberTrail._samplingFrequency = 120;
            _saberTrail._granularity = 45;
            _saberTrail.Setup(coreColor, _movementData);
            gameObject.SetActive(true);
        }

        private new void Update() {
            UpdateMaterialIfDirty();
            UpdateMovementData();
        }

        private void OnValidate() {
            UpdateMaterialIfDirty();
        }

        #endregion

        #region Properties

        [SerializeField]
        private Color coreColor = Color.red;

        [SerializeField]
        private float coreIntensity = 1f;

        public Color CoreColor {
            get => coreColor;
            set {
                if (coreColor == value) return;
                coreColor = value;
                _saberTrail.Setup(value, _movementData);
                SetPropertyBlockDirty();
            }
        }

        public float CoreIntensity {
            get => coreIntensity;
            set {
                if (Math.Abs(coreIntensity - value) < 0.001) return;
                coreIntensity = value;
                SetPropertyBlockDirty();
            }
        }

        #endregion

        #region Trail

        public float TrailLength {
            get => _saberTrail._trailDuration;
            set {
                _saberTrail._trailDuration = value;
                _saberTrail.Init();
            }
        }

        public float TrailOpacity {
            get => _saberTrail._color.a;
            set {
                var color = coreColor;
                color.a = Mathf.Lerp(0f, color.a, value);
                _saberTrail._color = color;
            }
        }

        public bool TrailEnabled {
            get => _saberTrail.enabled;
            set => _saberTrail.enabled = value;
        }

        private readonly SaberMovementData _movementData = new();
        private SaberTrail _saberTrail = null!;

        private void UpdateMovementData() {
            var bottomPos = transform.position;
            var topPos = transform.forward + bottomPos;
            _movementData.AddNewData(topPos, bottomPos, TimeHelper.GetShaderTimeValue());
        }

        private static SaberTrailRenderer InstantiateTrailRenderer() {
            var prefab = Resources
                .FindObjectsOfTypeAll<SaberTrailRenderer>()
                .First(x => x.name == "SaberTrailRenderer");
            return Instantiate(prefab);
        }

        #endregion

        #region MaterialPropertyBlock

        private static readonly int coreColorPropertyId = Shader.PropertyToID("_CoreColor");
        private static readonly int coreIntensityPropertyId = Shader.PropertyToID("_CoreIntensity");

        private MaterialPropertyBlock _propertyBlock = null!;
        private bool _propertyBlockDirty = true;

        private void SetPropertyBlockDirty() {
            _propertyBlockDirty = true;
        }

        private void UpdateMaterialIfDirty() {
            if (!_propertyBlockDirty) return;
            _propertyBlock.SetColor(coreColorPropertyId, CoreColor);
            _propertyBlock.SetFloat(coreIntensityPropertyId, CoreIntensity);
            ApplyBlock(_propertyBlock);
            _propertyBlockDirty = false;
        }

        protected abstract void ApplyBlock(MaterialPropertyBlock block);

        #endregion
    }
}