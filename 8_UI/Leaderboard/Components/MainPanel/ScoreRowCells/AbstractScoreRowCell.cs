using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class AbstractScoreRowCell : ReeUIComponentV2 {
        #region RootNode

        [UIComponent("root-node"), UsedImplicitly]
        public LayoutElement rootNode;

        public void SetCellWidth(float value) {
            _cellWidth = value;
            UpdateRootNode();
        }

        public void SetActive(bool value) {
            if (_isActive.Equals(value)) return;
            _isActive = value;
            UpdateRootNode();
        }

        #endregion

        #region UpdateRootNode

        private bool _isActive = true;
        private float _cellWidth = 1f;

        private void UpdateRootNode() {
            if (!IsHierarchySet) return;
            rootNode.preferredWidth = _cellWidth;
            rootNode.gameObject.SetActive(_isActive);
            gameObject.SetActive(_isActive);
        }

        protected override void OnInitialize() {
            UpdateRootNode();
        }

        #endregion

        #region GetPreferredWidth

        public bool isEmpty = true;

        public void MarkEmpty() {
            isEmpty = true;
        }

        public float GetPreferredWidth() {
            return isEmpty ? 0.0f : CalculatePreferredWidth();
        }

        #endregion

        #region Abstract

        public abstract void SetAlpha(float value);

        protected abstract float CalculatePreferredWidth();

        #endregion
    }
}