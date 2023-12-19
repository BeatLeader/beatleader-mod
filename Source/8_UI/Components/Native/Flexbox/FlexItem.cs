using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal enum AlignSelf {
        Auto = 0,
        Stretch,
        FlexStart,
        FlexEnd,
        Center
    }

    //forked and modified from https://github.com/JiangJie/flexbox4unity/blob/main/src/FlexItem.cs
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    internal sealed class FlexItem : UIBehaviour {
        #region Serialized Fields

        [SerializeField] 
        private float flexGrow;

        [SerializeField] 
        private float flexShrink = 1;

        [SerializeField] 
        private Vector2 flexBasis = new(-1f, -1f);

        [SerializeField] 
        private Vector2 minSize = new(-1f, -1f);

        [SerializeField] 
        private Vector2 maxSize = new(-1f, -1f);

        [SerializeField] 
        private AlignSelf alignSelf;

        [SerializeField]
        private int order;

        #endregion

        #region UI Properties

        public float FlexGrow {
            get => Math.Max(flexGrow, 0f);
            set => flexGrow = value;
        }

        public float FlexShrink {
            get => Math.Max(flexShrink, 0f);
            set => flexShrink = value;
        }

        public Vector2 FlexBasis {
            get => flexBasis;
            set => flexBasis = value;
        }

        public Vector2 MinSize {
            get => minSize;
            set => minSize = value;
        }

        public Vector2 MaxSize {
            get => maxSize;
            set => maxSize = value;
        }

        public AlignSelf AlignSelf {
            get => alignSelf;
            set => alignSelf = value;
        }

        public int Order {
            get => order;
            set => order = value;
        }

        #endregion

        #region UI Rebuild

        private DrivenRectTransformTracker _tracker;

        private void TryMakeContainerRebuild() {
            if (transform.parent == null || !transform.parent.TryGetComponent<FlexContainer>(out var container)) return;

            _tracker.Clear();
            _tracker.Add(
                this, transform as RectTransform,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.AnchoredPosition
            );

            container.SetDirty();
        }

        #endregion

        #region Unity Events

        protected override void OnEnable() {
            base.OnEnable();
            TryMakeContainerRebuild();
        }

        protected override void OnDisable() {
            _tracker.Clear();
            TryMakeContainerRebuild();
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties() {
            base.OnDidApplyAnimationProperties();
            TryMakeContainerRebuild();
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            TryMakeContainerRebuild();
        }

        #endregion
    }
}