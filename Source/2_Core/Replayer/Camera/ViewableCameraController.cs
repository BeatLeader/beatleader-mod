using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Replayer {
    internal class ViewableCameraController : MonoBehaviour, IViewableCameraController {
        public IVRControllersProvider ControllersProvider { get; set; } = null!;
        public Transform CameraContainer {
            get => _cameraContainer;
            set => _cameraContainer = value ?? transform;
        }
        public Camera? Camera => _camera;
        public IList<ICameraView> Views { get; } = new List<ICameraView>();
        public ICameraView? SelectedView => _view;

        public event Action<ICameraView>? CameraViewChangedEvent;
        public event Action? CameraViewProcessedEvent;

        private ICameraView? _view;
        private Camera? _camera;
        private Transform _cameraContainer = null!;
        private bool _cameraIsNotNull;
        private bool _viewIsNotNull;

        private void Awake() {
            _cameraContainer = transform;
        }

        public void SetCamera(Camera? camera) {
            _camera = camera;
            _cameraIsNotNull = camera != null;
            UpdateView();
        }

        public void SetEnabled(bool enabled = true) {
            gameObject.SetActive(enabled);
        }

        public bool SetView(string name) {
            var view = Views.FirstOrDefault(x => x.Name == name);
            if (view == null) return false;
            _view = view;
            _viewIsNotNull = view != null;
            CameraViewChangedEvent?.Invoke(view!);
            UpdateView();
            return true;
        }

        private void UpdateView() {
            _view?.ProcessView(this);
            CameraViewProcessedEvent?.Invoke();
        }

        private void Update() {
            if (!_cameraIsNotNull || !_viewIsNotNull || !_view!.Update) return;
            UpdateView();
        }
    }
}
