using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Attributes;
using BeatLeader.Utils;

namespace BeatLeader.Interop {
    [PluginInterop("SongCore")]
    internal static class SongCoreInterop {
        #region Init

        [PluginAssembly]
        private static readonly Assembly _assembly;

        [PluginType("SongCore.Loader")]
        private static readonly Type _loaderType;

        [PluginType("SongCore.Collections")]
        private static readonly Type _collectionsType;

        [PluginState]
        private static readonly bool _isInitialized;

        private static MethodInfo _loaderRefreshSongsMethod;
        private static MethodInfo _collectionsRetrieveDataMethod;
        private static MethodInfo _collectionsGetCapabilitiesMethod;
        private static FieldInfo _loaderInstanceField;
        private static FieldInfo _dataRequirementsField;
        private static FieldInfo _dataAdditionalDiffField;

        [InteropEntry]
        private static void Init() {
            _loaderInstanceField = _loaderType
                .GetField("Instance", BindingFlags.Static | BindingFlags.Public);
            _loaderRefreshSongsMethod = _loaderType
                .GetMethod("RefreshSongs", ReflectionUtils.DefaultFlags);
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
        }

        #endregion

        #region TryRefreshSongs

        public static bool TryRefreshSongs(bool fullRefresh) {
            if (!_isInitialized) return false;
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
            if (!_isInitialized) {
                requirements = null;
                return false;
            }
            try {
                var data = _collectionsRetrieveDataMethod.Invoke(null, new object[] { beatmap });
                if (data == null) {
                    requirements = null;
                    return false;
                }
                var reqData = _dataAdditionalDiffField.GetValue(data);
                if (reqData == null) {
                    requirements = null;
                    return false;
                }
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
            if (!_isInitialized) {
                capabilities = null;
                return false;
            }
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

        #region ValidateRequirements

        public static bool ValidateRequirements(IDifficultyBeatmap beatmap) {
            return TryGetBeatmapRequirements(beatmap, out var requirements)
                && TryGetCapabilities(out var capabilities)
                && requirements.All(x => capabilities.Contains(x));
        }

        #endregion
    }
}