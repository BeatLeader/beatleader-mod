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
            Plugin.Log.Notice("Found camera2");

            var cameras = Resources.FindObjectsOfTypeAll(type);
            if (cameras == null) return;
            Plugin.Log.Notice("Trying to get camera2 cameras");

            foreach (var cam in cameras)
            {
                var renderText = GetCameraRenderTexture(cam);
                var name = cam.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public).GetValue(cam);
                Plugin.Log.Info($"Found {name} camera, render size: {renderText.width}x{renderText.height}");
                if (renderText.width >= _uiManager.CanvasSize.x && renderText.height >= _uiManager.CanvasSize.y)
                {
                    Plugin.Log.Notice($"Found {name} fullscreen camera, installing temp lock...");
                    InstallTempLockOnCamera(cam);
                }
            }
            Plugin.Log.Notice("Camera2 patch done.");
        }

        private void InstallTempLockOnCamera(object camera)
        {
            var camGo = (GameObject)camera.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance).GetValue(camera);
            TempLock.Create(camGo);
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
