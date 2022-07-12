using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Managers;
using BeatLeader.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader
{
    public class UI2DManager : MonoBehaviour, ITickable
    {
        [Inject] private readonly InputManager _inputManager;

        private CanvasScaler _canvasScaler;
        private Canvas _canvas;
        private PointerEventData _pointerEventData;
        private GraphicRaycaster _raycaster;
        private RectTransform _canvasRect;

        private List<(GameObject, Transform)> _objects = new();

        public KeyCode hideUIHotkey = KeyCode.H;
        public KeyCode hideCursorHotkey = KeyCode.C;

        public event Action<bool> onUIVisibilityChanged;
        public event Action<bool> onCursorVisibilityChanged;
        public event Action<List<RaycastResult>> onRaycast;

        public List<RaycastResult> lastRaycasts { get; private set; } = new();
        public Vector2 canvasSize => _canvasRect.sizeDelta;
        public float scaleFactor => _canvasScaler.scaleFactor;

        private void Start()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvasScaler = gameObject.AddComponent<CanvasScaler>();
            _canvasRect = gameObject.GetOrAddComponent<RectTransform>();
            _raycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.SetupAsViewController();
            _canvasScaler.scaleFactor = 5f;
            _canvasScaler.referenceResolution = canvasSize / scaleFactor;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            _pointerEventData = new PointerEventData(_inputManager.currentEventSystem);
            _inputManager.onEventSystemChanged += ChangePointerEventSystem;
        }
        public void Tick()
        {
            UpdatePointerData();
            lastRaycasts.Clear();
            _raycaster.Raycast(_pointerEventData, lastRaycasts);
            onRaycast?.Invoke(lastRaycasts);

            if (Input.GetKeyDown(hideUIHotkey))
            {
                gameObject.active = !gameObject.active;
                onUIVisibilityChanged?.Invoke(gameObject.active);
            }
            if (Input.GetKeyDown(hideCursorHotkey))
            {
                InputManager.SwitchCursor();
                onCursorVisibilityChanged?.Invoke(Cursor.visible);
            }
        }
        private void UpdatePointerData()
        {
            _pointerEventData.position = Input.mousePosition;
        }
        private void ChangePointerEventSystem(EventSystem eventSystem)
        {
            _pointerEventData = new PointerEventData(eventSystem);
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

        public static bool IsCoveredByAnotherObject(GameObject go, List<RaycastResult> result)
        {
            float idx = 0;
            float maxIdx = 0;
            result.ForEach(delegate (RaycastResult x)
            {
                if (x.gameObject == go) idx = x.displayIndex; //why index is float ??!
                maxIdx = x.displayIndex > maxIdx ? x.displayIndex : maxIdx;
            });
            return maxIdx > idx;
        }
    }
}
