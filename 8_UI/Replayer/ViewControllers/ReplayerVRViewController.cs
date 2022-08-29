﻿using System;
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

using VRUIControls;
using IPA.Utilities;

namespace BeatLeader.ViewControllers
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : MonoBehaviour
    {
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly MainCamera _camera;
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly InputManager _inputManager;

        private bool syncView
        {
            get => _syncView;
            set
            {
                _syncView = value;
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        [SerializeAutomatically] private static bool _syncView = true;

        [UIValue("rotation-menu")] private RotationMenu _rotationMenu;
        [UIValue("toolbar")] private Toolbar _toolbar;

        private FloatingScreen _floatingScreen;
        private List<int> _blockedRaycastLayers = new List<int> { 8, 9, 10, 11 };

        private void Start()
        {
            _rotationMenu = ReeUIComponentV2.Instantiate<RotationMenu>(transform);
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            var container = new GameObject("Container");
            var viewContainer = new GameObject("ViewContainer");
            viewContainer.transform.SetParent(container.transform, false);
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100, 55), false, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
            _floatingScreen.transform.SetParent(viewContainer.transform, false);
            _floatingScreen.transform.localPosition = new UnityEngine.Vector3(0, 1, 1.7f);
            _floatingScreen.transform.localEulerAngles = new UnityEngine.Vector3(50, 0, 0);
            _floatingScreen.ParseInObjectHierarchy(BSMLUtil.ReadViewDefinition<ReplayerVRViewController>(), this);

            var view = container.gameObject.AddComponent<RotatingVRView>();
            view.Init(_camera.transform, viewContainer.transform);
            _rotationMenu.view = view;
            _rotationMenu.EnableSync(syncView);
            _rotationMenu.OnViewSyncChanged += NotifyViewSyncChanged;

            _vrControllersManager.AttachToTheNode(XRNode.GameController, container.transform);
            _vrControllersManager.ShowMenuControllers();

            var comparableInputModule = Resources.FindObjectsOfTypeAll<ComparableVRInputModule>().FirstOrDefault();
            if (comparableInputModule == null) return;

            var raycaster = _floatingScreen.GetComponent<VRGraphicRaycaster>();
            raycaster.SetField("_blockingMask", (LayerMask)LayerMaskHelper.LayersToBit(_blockedRaycastLayers));

            //comparableInputModule.raycaster = raycaster;
            //comparableInputModule.comparator = new GameplayObjectsComparatorModule();
        }
        private void NotifyViewSyncChanged(bool state) => syncView = state;
    }
}