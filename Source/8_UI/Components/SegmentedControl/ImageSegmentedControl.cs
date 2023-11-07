using System.Collections.Generic;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// Implementation of <c>ImageSegmentedControlBase</c> with string as key
    /// </summary>
    internal class ImageSegmentedControl : ImageSegmentedControlBase<ImageSegmentedControl, string> {
        protected override LayoutGroupType LayoutGroup => LayoutGroupType.Horizontal;
    }

    /// <summary>
    /// Cell for <c>ImageSegmentedControlBase</c>
    /// </summary>
    internal class ImageSegmentedControlCell : SegmentedControlComponentBaseCell {
        #region Setup

        private ImageButton _button = null!;

        public void Init(Sprite sprite, ImageButtonParamsDescriptor desc) {
            _button.GrowOnHover = desc.GrowOnHover;
            _button.HoverLerpMul = desc.HoverLerpMul;
            _button.ActiveColor = desc.ActiveColor;
            _button.HoverColor = desc.HoverColor;
            _button.Color = desc.Color;
            _button.Pad = desc.Pad;
            _button.BaseScale = desc.BaseScale;
            _button.HoverScaleSum = desc.HoverScaleSum;
            _button.Image.Sprite = sprite;
        }

        public void Setup(ImageButton button) {
            _button = button;
            _button.Sticky = true;
            _button.InheritSize = true;
            _button.ClickEvent += HandleButtonToggled;
        }
        
        #endregion

        #region Callbacks

        private void HandleButtonToggled(bool state) {
            NotifyControlStateChanged();
        }
        
        public override void OnStateChange(bool state) {
            _button.Click(state, notifyListeners: false);
        }

        #endregion
    }

    /// <summary>
    /// Implementation of <c>SegmentedControlComponentBase</c> with icons as cells
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TKey">Item key</typeparam>
    internal abstract class ImageSegmentedControlBase<T, TKey> : SegmentedControlComponentBase<T, TKey, Sprite> where T : ReeUIComponentV3<T> {
        #region UI Properties

        [ExternalProperty("Items"), UsedImplicitly]
        protected IList<KeyValuePair<TKey, Sprite>> PreloadItems {
            set => value.ForEach(x => items.Add(x.Key, x.Value));
        }

        [ExternalProperty(prefix: "tab"), UsedImplicitly]
        public readonly ImageButtonParamsDescriptor cellParams = new();

        #endregion

        #region Setup

        protected override SegmentedControlComponentBaseCell ConstructCell(Sprite value) {
            if (DequeueReusableCell() is not ImageSegmentedControlCell cell) {
                var button = ImageButton.Instantiate(ContentTransform!);
                cell = button.Content!.AddComponent<ImageSegmentedControlCell>();
                cell.Setup(button);
            } 
            cell.Init(value, cellParams);
            return cell;
        }

        protected override void OnPropertySet() {
            Reload();
        }

        #endregion
    }
}