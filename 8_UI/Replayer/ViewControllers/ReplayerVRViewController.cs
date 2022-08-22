using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Replayer.Movement;
using BeatLeader.Components;
using BeatLeader.Replayer;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine;
using Zenject;
using HMUI;
using BeatSaberMarkupLanguage.FloatingScreen;

namespace BeatLeader.ViewControllers
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : MonoBehaviour
    {
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly MainCamera _camera;
        [Inject] private readonly DiContainer _container;

        private bool syncView
        {
            get => _syncView;
            set
            {
                _syncView = value;
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        [UIValue("rotation-menu")] private RotationMenu _rotationMenu;
        [UIValue("toolbar")] private Toolbar _toolbar;

        [SerializeAutomatically] private static bool _syncView = true;
        private FloatingScreen _floatingScreen;

        private void Start()
        {
            _rotationMenu = ReeUIComponentV2.Instantiate<RotationMenu>(transform);
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            var go = new GameObject("Container");
            var viewGo = new GameObject("RotatingView");
            viewGo.transform.SetParent(go.transform, false);
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100, 55), false, UnityEngine.Vector3.zero,
                UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(50, 0, 0)));
            _floatingScreen.transform.SetParent(viewGo.transform, false);
            _floatingScreen.transform.localPosition = new UnityEngine.Vector3(0, 1, 1.7f);
            _floatingScreen.ParseInObjectHierarchy(BSMLUtil.ReadViewDefinition<ReplayerVRViewController>(), this);

            var view = go.gameObject.AddComponent<RotatingVRView>();
            view.Init(_camera.transform, viewGo.transform);
            _rotationMenu.Init(view);
            _rotationMenu.EnableSync(syncView);
            _rotationMenu.OnViewSyncChanged += NotifyViewSyncChanged;

            _vrControllersManager.AttachToTheNode(XRNode.GameController, go.transform);
            _vrControllersManager.ShowMenuControllers();
        }
        private void NotifyViewSyncChanged(bool state) => syncView = state;
    }
}
