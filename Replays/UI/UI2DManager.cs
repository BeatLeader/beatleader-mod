using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.Replays.UI
{
    public class UI2DManager : MonoBehaviour
    {
        protected GameObject _rootContainer;
        protected Canvas _rootCanvas;

        public void Start()
        {
            _rootContainer = gameObject;
            _rootCanvas = _rootContainer.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _rootCanvas.scaleFactor = 300;
            _rootCanvas.referencePixelsPerUnit = 10;
        }
        public void AddObject(GameObject go)
        {
            if (_rootContainer != null)
            {
                go.transform.SetParent(_rootContainer.transform, false);
                if (go.GetComponent<GraphicRaycaster>() == null) 
                    go.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
