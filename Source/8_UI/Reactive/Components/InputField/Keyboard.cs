using System;
using System.Linq;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using UnityEngine;
using VRUIControls;
using Object = UnityEngine.Object;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IKeyboardController<in T> where T : IInputFieldController {
        event Action? KeyboardClosedEvent;

        void Setup(T? input);
        void SetActive(bool active);
        void Refresh();
    }

    internal class Keyboard : ReactiveComponent, IKeyboardController<IInputFieldController> {
        #region Keyboard

        private IInputFieldController InputField {
            get => _inputField ?? throw new UninitializedComponentException();
        }

        public event Action? KeyboardClosedEvent;

        private IInputFieldController? _inputField;

        void IKeyboardController<IInputFieldController>.Setup(IInputFieldController? input) {
            _inputField = input;
            Refresh();
        }

        void IKeyboardController<IInputFieldController>.SetActive(bool active) { }

        public void Refresh() {
            _okButton.interactable = InputField.CanProceed;
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _uiKeyboard.okButtonWasPressedEvent += HandleOkButtonPressed;
            _uiKeyboard.deleteButtonWasPressedEvent += HandleDeletePressed;
            _uiKeyboard.keyWasPressedEvent += HandleKeyPressed;
        }

        #endregion

        #region Construct

        public Image BackgroundImage => _backgroundImage;

        protected override float? DesiredHeight => 32f;
        protected override float? DesiredWidth => 96f;

        private HMUI.UIKeyboard _uiKeyboard = null!;
        private UnityEngine.UI.Button _okButton = null!;
        private Image _backgroundImage = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    new Dummy()
                        .With(
                            x => {
                                _uiKeyboard = InstantiateKeyboard();
                                //TODO: asm pub:
                                _okButton = _uiKeyboard.GetField<UnityEngine.UI.Button, HMUI.UIKeyboard>("_okButton");
                                _uiKeyboard.transform.SetParent(x.ContentTransform, false);
                            }
                        ).AsFlexItem(size: new() { x = 92f, y = 28f })
                }
            }.AsFlexGroup(padding: 2f).WithSizeDelta(96f, 32f).AsBlurBackground().Bind(ref _backgroundImage).Use();
        }

        private static HMUI.UIKeyboard InstantiateKeyboard() {
            var original = Resources.FindObjectsOfTypeAll<HMUI.UIKeyboard>().First();
            var clone = Object.Instantiate(original);
            var raycaster = clone.GetComponent<VRGraphicRaycaster>();
            BeatSaberUI.DiContainer.Inject(raycaster);
            return clone.GetComponent<HMUI.UIKeyboard>();
        }

        #endregion

        #region Callbacks

        private void HandleOkButtonPressed() {
            KeyboardClosedEvent?.Invoke();
        }

        private void HandleKeyPressed(char key) {
            if (!InputField.CanAppend(key.ToString())) return;
            InputField.Append(key.ToString());
            Refresh();
        }

        private void HandleDeletePressed() {
            if (!InputField.CanTruncate(1)) return;
            InputField.Truncate(1);
            Refresh();
        }

        #endregion
    }
}