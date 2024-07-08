using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagEditorPanel : ReactiveComponent {
        #region Tag

        public Func<string, ReplayTagValidationResult>? ValidationContract { get; set; }

        public string TagName { get; private set; } = string.Empty;
        public Color TagColor { get; private set; }
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
            return new Dummy {
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
                    }.WithListener(
                        x => x.Text,
                        x => {
                            TagName = x;
                            if (x.Length == 0) _validationResult = null;
                            NotifyPropertyChanged(nameof(TagName));
                            NotifyPropertyChanged(nameof(IsValid));
                        }
                    ).AsFlexItem(grow: 1f, size: new() { y = 8f }).Bind(ref _inputField),
                    //
                    new ColorPicker {
                        Color = ColorUtils.RandomColor()
                    }.WithListener(
                        x => x.Color,
                        x => {
                            TagColor = x;
                            NotifyPropertyChanged(nameof(TagColor));
                        }
                    ).AsFlexItem(size: "auto").Bind(ref _colorPicker)
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        #endregion
    }
}