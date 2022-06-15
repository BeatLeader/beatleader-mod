using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.UI;
using UnityEngine.XR;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class MultiplatformUIManager : MonoBehaviour
    {
        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly UI2DManager _ui2DManager;
        [Inject] protected readonly DiContainer _container;

        public IPlaybackViewController playbackViewController { get; protected set; }

        public void Start()
        {
            if (_inputManager.isInFPFC)
                playbackViewController = new GameObject("PlaybackNonVRViewController").AddComponent<PlaybackNonVRViewController>();
            else
                playbackViewController = new GameObject("PlaybackVRViewController").AddComponent<PlaybackVRViewController>();

            _container.Inject(playbackViewController);
            _container.Bind<IPlaybackViewController>().FromInstance(playbackViewController).AsSingle();
            playbackViewController.Init();

            Install(playbackViewController);
        }
        public void OnDestroy()
        {
            Destroy(playbackViewController.root);
        }
        public void EnableInterface()
        {
            playbackViewController.Enable();
        }
        public void DisableInterface()
        {
            playbackViewController.Disable();
        }
        protected void Install(IPlaybackViewController viewController)
        {
            if (_inputManager.isInFPFC)
            {
                _ui2DManager.AddObject(viewController.floatingScreen.gameObject);
            }
            else
            {
                _vrControllersManager.AttachToTheHand(XRNode.LeftHand, viewController.floatingScreen.transform);
            }
        }
    }
}
