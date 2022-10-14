using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Utils;
using IPA.Loader;

namespace BeatLeader.Interop {
    internal static class SongCoreInterop {
        #region Init

        private static Assembly _assembly;
        private static Type _loaderType;
        private static Type _collectionsType;
        private static MethodInfo _loaderRefreshSongsMethod;
        private static MethodInfo _collectionsRetrieveDataMethod;
        private static MethodInfo _collectionsGetCapabilitiesMethod;
        private static FieldInfo _loaderInstanceField;
        private static FieldInfo _dataRequirementsField;
        private static FieldInfo _dataAdditionalDiffField;

        public static void Init() {
            _assembly = PluginManager.GetPluginFromId("SongCore")?.Assembly;
            if (_assembly == null) return;

            try {
                _loaderType = _assembly.GetType("SongCore.Loader");
                _loaderInstanceField = _loaderType
                    .GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                _loaderRefreshSongsMethod = _loaderType
                    .GetMethod("RefreshSongs", ReflectionUtils.DefaultFlags);

                _collectionsType = _assembly.GetType("SongCore.Collections");
                _collectionsRetrieveDataMethod = _collectionsType
                    .GetMethod("RetrieveDifficultyData", BindingFlags.Static | BindingFlags.Public);

                var extraType = _assembly.GetType("SongCore.Data.ExtraSongData");
                var difficultyDataType = extraType.GetNestedType("DifficultyData");
                var requirementDataType = extraType.GetNestedType("RequirementData");

                _dataAdditionalDiffField = difficultyDataType
                    .GetField("additionalDifficultyData", ReflectionUtils.DefaultFlags);
                _dataRequirementsField = requirementDataType
                    .GetField("_requirements", ReflectionUtils.DefaultFlags);
                _collectionsGetCapabilitiesMethod = _collectionsType
                    .GetProperty("capabilities", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to resolve SongCore data, some features may not work properly!");
            }
        }

        #endregion

        #region TryRefreshSongs

        public static bool TryRefreshSongs(bool fullRefresh) {
            try {
                var loaderInstance = _loaderInstanceField.GetValue(null);
                _loaderRefreshSongsMethod.Invoke(loaderInstance, new object[] { fullRefresh });
                return true;
            } catch (Exception e) {
                Plugin.Log.Debug($"RefreshSongs failed: \r\n {e}");
                return false;
            }
        }

        #endregion

        #region TryGetBeatmapRequirements

        public static bool TryGetBeatmapRequirements(IDifficultyBeatmap beatmap, out string[] requirements) {
            try {
                var data = _collectionsRetrieveDataMethod.Invoke(null, new object[] { beatmap });
                var reqData = _dataAdditionalDiffField.GetValue(data);
                requirements = (string[])_dataRequirementsField.GetValue(reqData);
                return true;
            } catch (Exception e) {
                Plugin.Log.Error($"GetRequirements failed: \r\n {e}");
                requirements = null;
                return false;
            }
        }

        #endregion

        #region TryGetCapabilities

        public static bool TryGetCapabilities(out IReadOnlyCollection<string> capabilities) {
            try {
                capabilities = (IReadOnlyCollection<string>)_collectionsGetCapabilitiesMethod.Invoke(null, null);
                return true;
            } catch (Exception e) {
                Plugin.Log.Error($"GetCapabilities failed: \r\n {e}");
                capabilities = null;
                return false;
            }
        }

        #endregion
    }
}