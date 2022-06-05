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

        protected IPlaybackViewController _playbackViewController;

        public IPlaybackViewController playbackViewController => _playbackViewController;

        public void Start()
        {
            if (_inputManager.isInFPFC)
                _playbackViewController = new GameObject("PlaybackNonVRViewController").AddComponent<PlaybackNonVRViewController>();
            else
                _playbackViewController = new GameObject("PlaybackVRViewController").AddComponent<PlaybackVRViewController>();

            _container.Inject(_playbackViewController);
            _container.Bind<IPlaybackViewController>().FromInstance(_playbackViewController).AsSingle();
            _playbackViewController.Init();

            Install(_playbackViewController);
        }
        public void OnDestroy()
        {
            Destroy(_playbackViewController.root);
        }
        public void EnableInterface()
        {
            _playbackViewController.Enable();
        }
        public void DisableInterface()
        {
            _playbackViewController.Disable();
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
