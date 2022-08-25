using System;
using IPA.Loader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using System.Collections;

namespace BeatLeader.Replayer
{
    public class Camera2Tool : MonoBehaviour //MOORE REFLECTION FOR REFLECTION GOD!
        //removed since kinsi made beat leader support
    {
        public class Cam2Info
        {
            public Cam2Info(Camera camera, object cam2, object smoothfollow, string name, bool isFPV, bool supportsInReplay)
            {
                this.camera = camera;
                this.cam2 = cam2;
                this.smoothfollow = smoothfollow;
                this.name = name;
                this.isFPV = isFPV;
                this.supportsInReplay = supportsInReplay;
            }

            public readonly string name;
            public readonly bool isFPV;
            public readonly bool supportsInReplay;
            public readonly object cam2;
            public readonly object smoothfollow;
            public readonly Camera camera;

            public override string ToString()
            {
                return $"{name}|{supportsInReplay}";
            }
        }

        public bool Detected => _detected;
        public List<Cam2Info> LoadedCameras => _camerasInfos;

        private List<Cam2Info> _camerasInfos = new();
        private bool _detected;
        private int _replaySceneId = 7;

        #region Reflection stuff

        private Type _cam2Type;
        private Type _scenesManagerType;
        private PropertyInfo _cam2nameInfo;
        private PropertyInfo _cam2cameraInfo;
        private PropertyInfo _cam2settingsInfo;
        private FieldInfo _smoothfollowParentInfo;
        private PropertyInfo _gameObjectInfo;
        private FieldInfo _settingsCamTypeInfo;
        private PropertyInfo _settingsSmoothfollowInfo;
        private PropertyInfo _scenesManagerSettingsInfo;
        private FieldInfo _scenesSettingsScenesInfo;

        #endregion

        public void SetSmoothfollowParent(object smoothfollow, Transform parent)
        {
            if (smoothfollow == null) return;
            _smoothfollowParentInfo.SetValue(smoothfollow, parent);
        }
        public Transform GetSmoothfollowParent(object smoothfollow)
        {
            return (Transform)_smoothfollowParentInfo.GetValue(smoothfollow);
        }

        private void Awake()
        {
            string failReason;
            if ((failReason = Init()) != string.Empty)
            {
                Plugin.Log.Warn(failReason);
                return;
            }

            /*foreach (var item in GetLoadedCameras())
            {
                if ((failReason = TryLoadInfo(item, out var info)) != string.Empty)
                {
                    Plugin.Log.Notice(failText + failReason);
                    continue;
                }
                _camerasInfos.Add(info);
                Plugin.Log.Notice("[Patcher] Found " + info.ToString());
            }*/
        }
        private string Init()
        {
            var emptyReason = string.Empty;
            try
            {
                var pluginInfo = PluginManager.GetPluginFromId("Camera2");
                if (pluginInfo == null) return emptyReason;
                _detected = true;

                Plugin.Log.Notice("[Patcher] Detected camera2 plugin");
                /*Plugin.Log.Notice("[Patcher] Scanning types");

                _cam2Type = pluginInfo.Assembly.GetType("Camera2.Behaviours.Cam2");
                _scenesManagerType = pluginInfo.Assembly.GetType("Camera2.Managers.ScenesManager");
                //_smoothfollowType = pluginInfo.Assembly.GetType("Camera2.Configuration.Settings_Smoothfollow");

                _cam2nameInfo = _cam2Type.GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                _cam2settingsInfo = _cam2Type.GetProperty("settings", BindingFlags.NonPublic | BindingFlags.Instance);
                _cam2cameraInfo = _cam2Type.GetProperty("UCamera", BindingFlags.NonPublic | BindingFlags.Instance);

                _gameObjectInfo = typeof(Component).GetProperty("gameObject",  BindingFlags.Public | BindingFlags.Instance);
                _settingsSmoothfollowInfo = _cam2settingsInfo.PropertyType.GetProperty("Smoothfollow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _smoothfollowParentInfo = _settingsSmoothfollowInfo.PropertyType.GetField("parent", BindingFlags.NonPublic | BindingFlags.Instance);
                _settingsCamTypeInfo = _cam2settingsInfo.PropertyType.GetField("_type",  BindingFlags.NonPublic | BindingFlags.Instance);
                _scenesManagerSettingsInfo = _scenesManagerType.GetProperty("settings",  BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                _scenesSettingsScenesInfo = _scenesManagerSettingsInfo.PropertyType.GetField("scenes",  BindingFlags.Public | BindingFlags.Instance);

                Plugin.Log.Notice("[Patcher] Scanning done");*/

                return emptyReason;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private string TryLoadInfo(object camera2, out Cam2Info info)
        {
            var emtpyReason = string.Empty;
            try
            {
                var settings = _cam2settingsInfo.GetValue(camera2);

                var name = ((string)_cam2nameInfo.GetValue(camera2)).Replace("Cam2_", "");
                var isFPV = (int)_settingsCamTypeInfo.GetValue(settings) == 0;
                var supportsReplay = IsCameraSupportsReplay(name);
                var camera = (Camera)_cam2cameraInfo.GetValue(camera2);
                var smoothfollow = _settingsSmoothfollowInfo.GetValue(settings);

                info = new Cam2Info(camera, camera2, smoothfollow, name, isFPV, supportsReplay);
                return emtpyReason;
            }
            catch (Exception ex)
            {
                info = null;
                return ex.Message;
            }
        }
        private bool IsCameraSupportsReplay(string name)
        {
            var sceneSettings = _scenesManagerSettingsInfo.GetValue(null);
            var scenes = (IEnumerable)_scenesSettingsScenesInfo.GetValue(sceneSettings);

            foreach (var item in scenes)
            {
                var key = (int)item.GetType().GetField("key", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item);
                var value = (List<string>)item.GetType().GetField("value", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item);
                if (key == _replaySceneId)
                {
                    return value.Contains(name);
                }
            }

            return false;
        }
        private object[] GetLoadedCameras()
        {
            return Resources.FindObjectsOfTypeAll(_cam2Type);
        }
    }
}
