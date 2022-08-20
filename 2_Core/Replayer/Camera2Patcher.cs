using System;
using IPA.Loader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    internal class Camera2Patcher : MonoBehaviour
    {
        [Inject] private readonly UI2DManager _uiManager;

        private void Awake()
        {
            var type = GetCam2Type();
            if (type == null) return;

            var cameras = Resources.FindObjectsOfTypeAll(type);
            if (cameras == null) return;

            foreach (var cam in cameras)
            {
                var renderText = GetCameraRenderTexture(cam);
                if (renderText.width == _uiManager.CanvasSize.x && renderText.height == _uiManager.CanvasSize.y)
                {
                    DisableCamera(cam);
                }
            }
        }

        private void DisableCamera(object camera)
        {
            var camGo = (GameObject)camera.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance).GetValue(camera);
            camGo.SetActive(false);
        }
        private RenderTexture GetCameraRenderTexture(object camera)
        {
            try
            {
                return (RenderTexture)camera.GetType().GetProperty("renderTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(camera);
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"Unable to resolve RenderTexture from {camera}");
                return null;
            }
        }
        private Type GetCam2Type()
        {
            return PluginManager.GetPluginFromId("Camera2")?.Assembly.GetType("Camera2.Behaviours.Cam2");
        }
    }
}
