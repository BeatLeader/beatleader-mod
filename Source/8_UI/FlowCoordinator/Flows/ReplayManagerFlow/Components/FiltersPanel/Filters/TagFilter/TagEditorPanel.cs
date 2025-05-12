using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;
using ColorUtils = BeatLeader.Utils.ColorUtils;

namespace BeatLeader.UI.Hub {
    internal class TagEditorPanel : ReactiveComponent {
        #region Tag

        public Func<string, ReplayTagValidationResult>? ValidationContract { get; set; }

        public string TagName => _inputField.Text;
        public Color TagColor => _colorPicker.Color;
        public bool IsValid => _validationResult?.Ok ?? false;

        public void Clear() {
            _colorPicker.Color = ColorUtils.RandomColor();
            _inputField.ClearText();
        }

        #endregion

        #region Construct

        private ReplayTagValidationResult? _validationResult;
        private InputField _inputField = null!;
        private ColorPicker _colorPicker = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new InputField {
                            Icon = GameResources.Sprites.EditIcon,
                            Placeholder = "Enter Name",

                            TextApplicationContract = x => {
                                _validationResult = ValidationContract?.Invoke(x);
                                NotifyPropertyChanged(nameof(IsValid));

                                return _validationResult?.Ok ?? false;
                            },

                            Keyboard = new KeyboardModal<Keyboard, InputField>()
                        }
                        .WithListener(
                            x => x.Text,
                            x => {
                                if (x.Length == 0) {
                                    _validationResult = null;
                                }
                                NotifyPropertyChanged(nameof(TagName));
                                NotifyPropertyChanged(nameof(IsValid));
                            }
                        )
                        .AsFlexItem(flexGrow: 1f, flexShrink: 1f)
                        .Bind(ref _inputField),

                    new ColorPicker {
                            Color = ColorUtils.RandomColor()
                        }
                        .WithListener(
                            x => x.Color,
                            _ => NotifyPropertyChanged(nameof(TagColor))
                        )
                        .AsFlexItem()
                        .Bind(ref _colorPicker)
                }
            }.AsFlexGroup(gap: 1f).AsFlexItem(size: new() { y = 8f }).Use();
        }

        #endregion
    }
}