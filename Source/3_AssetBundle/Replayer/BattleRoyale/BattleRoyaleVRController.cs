using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal abstract class BattleRoyaleVRController : VRController {
        #region Unity Events

        private void Awake() {
            _propertyBlock = new MaterialPropertyBlock();
            _saberTrail = gameObject.AddComponent<SaberTrail>();
            _saberTrail._trailRendererPrefab = TrailRendererPrefab;
            _saberTrail._trailDuration = 0.4f;
            _saberTrail._whiteSectionMaxDuration = 0.001f;
            _saberTrail._samplingFrequency = 120;
            _saberTrail._granularity = 45;
            _saberTrail.Setup(coreColor, _movementData);

            // Disabling so the trail won't initialize until turned back on
            _saberTrail.enabled = false;
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

        [SerializeField] private Color coreColor = Color.red;

        [SerializeField] private float coreIntensity = 1f;

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

        private bool _initialized;

        public void Init(DiContainer container) {
            container.Inject(_saberTrail);
            // Will be initialized later if not yet turned on
            if (TrailEnabled) {
                _saberTrail.enabled = true;
            }

            _initialized = true;
        }

        public float TrailLength {
            get => _saberTrail._trailDuration;
            set {
                _saberTrail._trailDuration = value;
                if (_saberTrail._trailRenderer != null) {
                    _saberTrail.Init();
                }
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
            get;
            set {
                if (_initialized) {
                    // SaberTrail disables the renderer automatically
                    _saberTrail.enabled = value;
                }

                field = value;
            }
        }

        private readonly SaberMovementData _movementData = new();
        private SaberTrail _saberTrail = null!;

        private void UpdateMovementData() {
            var bottomPos = transform.position;
            var topPos = transform.forward + bottomPos;
            _movementData.AddNewData(topPos, bottomPos, TimeHelper.GetShaderTimeValue());
        }

        private static SaberTrailRenderer? TrailRendererPrefab {
            get {
                if (field == null) {
                    var prefab = Resources
                        .FindObjectsOfTypeAll<SaberTrailRenderer>()
                        .First(x => x.name == "SaberTrailRenderer");

                    // We create a renamed prefab to avoid shenanigans
                    field = Instantiate(prefab);
                    field.name = "BattleRoyaleTrailRenderer";
                }

                return field;
            }
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