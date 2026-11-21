using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Attributes;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Interop {
    [PluginInterop("Vivify")]
    internal static class VivifyInterop {
        [PluginState]
        public static bool IsInstalled { get; private set; }

        [PluginType("Vivify.Managers.PrefabManager")]
        private static Type _prefabManagerType = null!;

        [PluginType("Vivify.Managers.InstantiatedPrefab")]
        private static Type _instantiatedPrefabType = null!;

        [PluginType("Vivify.Controllers.Sync.SyncController")]
        private static Type _syncControllerType = null!;

        private static FieldInfo _prefabsField = null!;
        private static PropertyInfo _prefabObjectProperty = null!;
        private static PropertyInfo _songTimeProperty = null!;
        private static MethodInfo _setStartTimeMethod = null!;

        [InteropEntry]
        private static void Init() {
            _prefabsField = _prefabManagerType.GetField("_prefabs", ReflectionUtils.DefaultFlags)
                ?? throw new MissingFieldException(_prefabManagerType.Name, "_prefabs");
            _prefabObjectProperty = _instantiatedPrefabType.GetProperty("GameObject", ReflectionUtils.DefaultFlags)
                ?? throw new MissingMemberException(_instantiatedPrefabType.Name, "GameObject");
            _songTimeProperty = _syncControllerType.GetProperty("SongTime", ReflectionUtils.DefaultFlags)
                ?? throw new MissingMemberException(_syncControllerType.Name, "SongTime");
            _setStartTimeMethod = _syncControllerType.GetMethod("SetStartTime", ReflectionUtils.DefaultFlags)
                ?? throw new MissingMethodException(_syncControllerType.Name, "SetStartTime");
        }

        public static List<GameObject> GetPrefabs(DiContainer container) {
            var prefabs = new List<GameObject>();
            if (!IsInstalled || container.TryResolve(_prefabManagerType) is not { } prefabManager) return prefabs;
            foreach (var prefab in ((IDictionary)_prefabsField.GetValue(prefabManager)).Values) {
                prefabs.Add((GameObject)_prefabObjectProperty.GetValue(prefab));
            }
            return prefabs;
        }

        public static bool TryGetSyncTime(GameObject gameObject, out float time) {
            var sync = gameObject.GetComponentInChildren(_syncControllerType, true);
            time = sync == null ? 0f : (float)_songTimeProperty.GetValue(sync);
            return sync != null;
        }

        public static bool SetSyncTime(Component component, float time) {
            var sync = component.GetComponent(_syncControllerType);
            if (sync == null) return false;
            _setStartTimeMethod.Invoke(sync, new object[] { time });
            return true;
        }
    }
}
