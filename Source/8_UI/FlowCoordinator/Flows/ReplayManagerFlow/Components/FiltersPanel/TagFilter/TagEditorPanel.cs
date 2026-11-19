using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using ColorUtils = BeatLeader.Utils.ColorUtils;

namespace BeatLeader.UI.Hub {
    internal class TagEditorPanel : ReactiveComponent {
        #region Tag

        public string TagName => _nameContext.Text ?? string.Empty;
        public Color TagColor => _colorContext.SelectedColor;
        public IState<bool> Validity => _validity;

        public void Clear() {
            _colorContext.SelectedColor = ColorUtils.RandomColor();
            _nameContext.Text = null;
        }

        #endregion

        #region Construct

        private readonly BsInputFieldContext _nameContext = new() {
            Validate = static x => x is not { Length: > 10 }
        };

        private readonly BsColorPickerContext _colorContext = new() { SelectedColor = ColorUtils.RandomColor() };
        private IState<bool> _validity = null!;

        private BsInputField _inputField = null!;
        private BsKeyboardModal _keyboardModal = null!;
        private BsColorPicker _colorPicker = null!;
        private BsColorCircleModal _colorModal = null!;

        protected override GameObject Construct() {
            _validity = _nameContext.Map(static x => x.Text != null && ReplayMetadataManager.ValidateTagName(x.Text).Ok);

            return new Layout {
                FlexController = {
                    Gap = 1f
                },

                FlexItem = {
                    Size = new() { y = 8f }
                },

                Children = {
                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f
                        },

                        Context = _nameContext,
                        Placeholder = "Enter Name",
                        Icon = GameResources.Sprites.EditIcon
                    }.Bind(ref _inputField),

                    new BsKeyboardModal {
                        Context = _nameContext
                    }.Bind(ref _keyboardModal),

                    new BsColorPicker {
                        Context = _colorContext
                    }.Bind(ref _colorPicker),

                    new BsColorCircleModal {
                        Context = _colorContext,
                        Title = "Select Color",
                        CircleSize = 54.pt,
                        PlacementData = new() {
                            Placement = RelativePlacement.Center,
                            Clip = true
                        }
                    }.Bind(ref _colorModal)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            _keyboardModal.PlacementAnchor = _inputField.ContentTransform;
            _colorModal.PlacementAnchor = _colorPicker.ContentTransform;
        }

        #endregion
    }
}