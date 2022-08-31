using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader
{
    [SerializeAutomatically]
    public class UI2DManager : MonoBehaviour
    {
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        public Vector2 CanvasSize => _canvasRect.sizeDelta;
        public float ScaleFactor => _canvasScaler.scaleFactor;
        public bool showUI
        {
            get => gameObject.activeSelf;
            set
            {
                _showUI = value;
                gameObject.SetActive(value);
                OnUIVisibilityChanged?.Invoke(value);
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        public event Action<bool> OnUIVisibilityChanged;

        [SerializeAutomatically] private static bool _showUI = true;
        private List<(GameObject, Transform)> _objects = new();
        private CanvasScaler _canvasScaler;
        private Canvas _canvas;
        private GraphicRaycaster _raycaster;
        private RectTransform _canvasRect;

        private void Awake()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvasScaler = gameObject.AddComponent<CanvasScaler>();
            _canvasRect = gameObject.GetOrAddComponent<RectTransform>();
            _raycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.SetupAsViewController();
            _canvasScaler.scaleFactor = 5f;
            _canvasScaler.referenceResolution = CanvasSize / ScaleFactor;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvas.sortingOrder = 1;
            gameObject.SetActive(_replayData.overrideSettings ? _replayData.settings.showUI : showUI);
        }
        public void AddObject(GameObject go)
        {
            _objects.Add((go, go.transform.parent));
            go.transform.SetParent(transform, false);
        }
        public void RemoveObject(GameObject go, bool returnToOriginalParent = true, Transform transform = null)
        {
            (GameObject, Transform) pair = (null, null);
            if ((pair = _objects.First(x => x.Item1 == go)) != (null, null))
            {
                go.transform.SetParent(returnToOriginalParent ? pair.Item2 : transform, false);
                _objects.Remove(pair);
            }
            else Plugin.Log.Warn($"{go} is not added to the 2D UI");
        }
    }
}
