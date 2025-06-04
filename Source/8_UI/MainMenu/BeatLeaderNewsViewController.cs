using HMUI;
using Reactive;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class BeatLeaderNewsViewController : ViewController {

        #region Setup

        private void Awake() {
            new Layout {
                Children = {
                    new TextNewsPanel()
                    //
                    //new Layout {
                    //    Children = {
                    //        new MapNewsPanel(),
                    //        new EventNewsPanel(),
                    //    }
                    //}.AsFlexGroup(
                    //    direction: FlexDirection.Column,
                    //    gap: 1f,
                    //    constrainHorizontal: false,
                    //    constrainVertical: false
                    //).Use(transform)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Row,
                gap: 1f
            )
            .AsFlexItem()
            .Use(transform);

            UpdateScreen();
        }

        private void OnEnable() {
            if (!_initialized) return;
            UpdateScreen();
        }

        private void OnDisable() {
            RevertScreenChanges();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            ReeModalSystem.CloseAll();
            gameObject.SetActive(false);
        }

        #endregion

        #region Screen Changes

        private static Vector2 TargetScreenSize => new Vector2(186, 80);

        private RectTransform _screenTransform;
        private Vector2 _originalScreenSize;
        private bool _initialized;

        private bool LazyInitializeScreen() {
            if (_initialized) return true;
            if (screen == null) return false;
            _screenTransform = screen.GetComponent<RectTransform>();
            _originalScreenSize = _screenTransform.sizeDelta;
            _initialized = true;
            return true;
        }

        private void UpdateScreen() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = TargetScreenSize;
        }

        private void RevertScreenChanges() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = _originalScreenSize;
        }

        #endregion
    }
}