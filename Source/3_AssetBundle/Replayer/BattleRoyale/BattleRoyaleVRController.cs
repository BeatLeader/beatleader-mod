using System;
using UnityEngine;

namespace BeatLeader {
    internal abstract class BattleRoyaleVRController : VRController {
        #region Unity Events

        private void Awake() {
            _propertyBlock = new MaterialPropertyBlock();
        }

        private new void Update() {
            UpdateMaterialIfDirty();
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