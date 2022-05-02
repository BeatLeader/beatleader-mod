using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replays.Interfaces;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackUIController : MonoBehaviour
    {
        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly BodyManager _bodyManager;

        protected const string _viewPath = Plugin.ResourcesPath + ".BSML.ReplayPlaybackUI.bsml";
        protected readonly Vector3 _defaultUIPos = new Vector3(-2.8f, -1.65f, 1f);

        protected FloatingScreen _floatingScreen;
        protected Camera _uiCamera;
        protected bool _initialized;

        public FloatingScreen floatingScreen => _floatingScreen;
        public Camera uiCamera => _uiCamera;
        public bool initialized => _initialized;

        public void Start()
        {
            _uiCamera = new GameObject("ReplayerGUICamera").AddComponent<Camera>();
            _uiCamera.depth = 15;
            _uiCamera.cullingMask = 2;
            _uiCamera.orthographicSize = 2;
            _uiCamera.orthographic = true;
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(70, 20), false, _defaultUIPos, Quaternion.identity);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), _viewPath), _floatingScreen.gameObject);
            //_floatingScreen.transform.SetParent(_uiCamera.transform, false);
            _floatingScreen.gameObject.layer = 1;

            _initialized = true;
        }
    }
}
