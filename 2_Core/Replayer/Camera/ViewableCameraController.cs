using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Replayer {
    internal class ViewableCameraController : MonoBehaviour, IViewableCameraController {
        public IVRControllersProvider ControllersProvider { get; private set; } = null!;
        public IList<ICameraView> Views { get; } = new List<ICameraView>();
        public ICameraView? SelectedView => _view;
        public Camera? Camera => _camera;
        public UnityEngine.Transform CameraContainer => transform;

        public event Action<ICameraView>? CameraViewChangedEvent;
        public event Action? CameraViewProcessedEvent;

        private ICameraView? _view;
        private Camera? _camera;
        private bool _cameraIsNotNull;
        private bool _viewIsNotNull;

        public void SetControllers(IVRControllersProvider provider) {
            ControllersProvider = provider;
        }

        public void SetCamera(Camera camera) {
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
