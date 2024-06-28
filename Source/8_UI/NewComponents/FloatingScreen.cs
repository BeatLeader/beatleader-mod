using BeatLeader.UI.Reactive;
using HMUI;
using UnityEngine;
using VRUIControls;
using Zenject;
using Screen = HMUI.Screen;

namespace BeatLeader.UI {
    internal class FloatingScreen : MonoBehaviour {
        #region Props

        public bool BlockRaycasts {
            get => !_raycaster.enabled;
            set {
                _raycaster.enabled = !value;
                if (_viewControllerRaycaster != null) {
                    _viewControllerRaycaster.enabled = !value;
                }
            }
        }

        public float CurvatureRadius {
            get => _curvedCanvasSettings.radius;
            set => _curvedCanvasSettings.SetRadius(value);
        }

        #endregion

        #region Setup

        private CurvedCanvasSettings _curvedCanvasSettings = null!;
        private VRGraphicRaycaster _raycaster = null!;
        private VRGraphicRaycaster _viewControllerRaycaster = null!;
        private Screen _screen = null!;

        private void Awake() {
            ReactiveUtils.AddCanvas(gameObject);
            _screen = gameObject.AddComponent<Screen>();
            _curvedCanvasSettings = gameObject.AddComponent<CurvedCanvasSettings>();
            CurvatureRadius = 0f;
            //
            _raycaster = gameObject.AddComponent<VRGraphicRaycaster>();
            var context = FindObjectOfType<Context>();
            context.Container.Inject(_raycaster);
            //
            transform.localScale = Vector3.one * 0.02f;
        }

        public void SetRootViewController(ViewController controller, ViewController.AnimationType animationType) {
            _screen.SetRootViewController(controller, animationType);
            _viewControllerRaycaster = controller.GetComponent<VRGraphicRaycaster>();
        }

        #endregion
    }
}