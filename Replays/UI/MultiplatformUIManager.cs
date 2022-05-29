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
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class MultiplatformUIManager : MonoBehaviour
    {
        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly DiContainer _container;

        protected IPlaybackViewController _playbackViewController;

        public IPlaybackViewController playbackViewController => _playbackViewController;

        public void Start()
        {
            if (_inputManager.currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                _playbackViewController = new GameObject("PlaybackNonVRViewController").AddComponent<PlaybackNonVRViewController>();
            }
            else if (_inputManager.currentInputSystem == InputManager.InputSystemType.VR)
            {
                _playbackViewController = new GameObject("PlaybackVRViewController").AddComponent<PlaybackVRViewController>();
            }
            _container.Inject(_playbackViewController);
            _container.Bind<IPlaybackViewController>().FromInstance(_playbackViewController).AsSingle();
            _playbackViewController.Init();
        }
    }
}
