using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Replays.Movement;
using BeatLeader.Components;
using BeatLeader.Replays;
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
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : MonoBehaviour
    {
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly MainCamera _camera;
        [Inject] private readonly DiContainer _container;

        [UIValue("toolbar")] private Toolbar _toolbar;

        private FloatingScreen _floatingScreen;

        private void Start()
        {
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            var go = new GameObject("Container");
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100, 45), false, UnityEngine.Vector3.zero,
                UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(50, 0, 0)));
            _floatingScreen.transform.SetParent(go.transform, false);
            _floatingScreen.transform.localPosition = new UnityEngine.Vector3(0, 1, 2);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(GetType().Assembly,
                GetType().GetCustomAttribute<ViewDefinitionAttribute>().Definition), _floatingScreen.gameObject, this);

            //_floatingScreen.gameObject.AddComponent<FollowingVRView>().Init(_camera.transform, go.transform);

            //_vrControllersManager.AttachToTheNode(XRNode.LeftHand, _floatingScreen.transform);
            _vrControllersManager.ShowMenuControllers();
        }
    }
}
