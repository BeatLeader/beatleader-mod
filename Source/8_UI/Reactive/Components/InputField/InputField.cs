using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IInputFieldController {
        bool CanProceed { get; }
        string Text { get; }

        event Action? TextClearedEvent;

        void Append(string text);
        void Truncate(int count);
        bool CanAppend(string text);
        bool CanTruncate(int count);
    }

    internal class InputField : ReactiveComponent, IInputFieldController {
        #region Input Field

        public string Text {
            get => _textArea.Text;
            private set {
                _textArea.Text = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxInputLength { get; set; } = int.MaxValue;
        public Func<string, bool>? TextApplicationContract { get; set; }
        bool IInputFieldController.CanProceed => TextApplicationContract?.Invoke(Text) ?? true;

        public event Action? TextClearedEvent;

        void IInputFieldController.Append(string text) {
            Text += text;
        }

        void IInputFieldController.Truncate(int count) {
            Text = Text.Remove(Text.Length - count, count);
        }

        bool IInputFieldController.CanAppend(string text) {
            return text.Length + Text.Length < MaxInputLength;
        }

        bool IInputFieldController.CanTruncate(int count) {
            return Text.Length - count >= 0;
        }

        public void ClearText() {
            Text = string.Empty;
            TextClearedEvent?.Invoke();
        }

        #endregion

        #region UI Props

        public string Placeholder {
            get => _textArea.Placeholder;
            set {
                _textArea.Placeholder = value;
                NotifyPropertyChanged();
            }
        }

        public Sprite? Icon {
            get => _textArea.Icon;
            set => _textArea.Icon = value;
        }

        #endregion

        #region Keyboard

        public IKeyboardController<IInputFieldController>? Keyboard { get; set; }

        private void SpawnKeyboard() {
            if (Keyboard == null) throw new UninitializedComponentException("Keyboard must be specified to use this component");
            Keyboard!.Setup(this);
            Keyboard.KeyboardClosedEvent += HandleKeyboardClosed;
            Keyboard.SetActive(true);
        }

        private void DespawnKeyboard(bool deactivate) {
            if (Keyboard == null) throw new UninitializedComponentException("Keyboard must be specified to use this component");
            Keyboard!.Setup(null);
            Keyboard.KeyboardClosedEvent -= HandleKeyboardClosed;
            if (deactivate) Keyboard.SetActive(false);
        }

        #endregion

        #region Construct

        private TextArea _textArea = null!;

        protected override GameObject Construct() {
            return new TextArea()
                .WithListener(
                    x => x.Focused,
                    HandleTextAreaFocusChanged
                )
                .WithListener(
                    x => x.Text,
                    x => {
                        if (x.Length > 0) return;
                        TextClearedEvent?.Invoke();
                        NotifyPropertyChanged(nameof(Text));
                    })
                .Bind(ref _textArea)
                .Use();
        }

        protected override void OnInitialize() {
            _textArea.ShowCaret = true;
            _textArea.ShowClearButton = true;
        }

        #endregion

        #region Callbacks

        private void HandleTextAreaFocusChanged(bool value) {
            if (value) SpawnKeyboard();
        }

        private void HandleKeyboardClosed() {
            _textArea.Focused = false;
            DespawnKeyboard(false);
        }

        #endregion
    }
}