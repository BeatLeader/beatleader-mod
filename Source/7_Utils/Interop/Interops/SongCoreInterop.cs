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
        private static readonly Assembly assembly = null!;

        [PluginType("SongCore.Loader")]
        private static readonly Type loaderType = null!;

        [PluginType("SongCore.Collections")]
        private static readonly Type collectionsType = null!;

        [PluginState]
        private static readonly bool isInitialized;

        private static MethodInfo? _loaderRefreshSongsMethod;
        private static MethodInfo? _collectionsRetrieveDataMethod;
        private static MethodInfo? _collectionsGetCapabilitiesMethod;
        private static FieldInfo? _loaderInstanceField;
        private static FieldInfo? _dataRequirementsField;
        private static FieldInfo? _dataAdditionalDiffField;

        [InteropEntry]
        private static void Init() {
            _loaderInstanceField = loaderType
                .GetField("Instance", BindingFlags.Static | BindingFlags.Public);
            _loaderRefreshSongsMethod = loaderType
                .GetMethod("RefreshSongs", ReflectionUtils.DefaultFlags);
            _collectionsRetrieveDataMethod = collectionsType
                .GetMethod("RetrieveDifficultyData", BindingFlags.Static | BindingFlags.Public);

            var extraType = assembly.GetType("SongCore.Data.ExtraSongData");
            var difficultyDataType = extraType.GetNestedType("DifficultyData");
            var requirementDataType = extraType.GetNestedType("RequirementData");

            _dataAdditionalDiffField = difficultyDataType
                .GetField("additionalDifficultyData", ReflectionUtils.DefaultFlags);
            _dataRequirementsField = requirementDataType
                .GetField("_requirements", ReflectionUtils.DefaultFlags);
            _collectionsGetCapabilitiesMethod = collectionsType
                .GetProperty("capabilities", BindingFlags.Static | BindingFlags.Public)!.GetGetMethod();
        }

        #endregion

        #region TryRefreshSongs

        public static bool TryRefreshSongs(bool fullRefresh) {
            if (!isInitialized) return false;
            try {
                var loaderInstance = _loaderInstanceField!.GetValue(null);
                _loaderRefreshSongsMethod!.Invoke(loaderInstance, new object[] { fullRefresh });
                return true;
            } catch (Exception e) {
                Plugin.Log.Debug($"RefreshSongs failed: \r\n {e}");
                return false;
            }
        }

        #endregion

        #region TryGetBeatmapRequirements

        public static bool TryGetBeatmapRequirements(IDifficultyBeatmap beatmap, out string[]? requirements) {
            requirements = null;
            if (!isInitialized) return false;
            try {
                var data = _collectionsRetrieveDataMethod!.Invoke(null, new object[] { beatmap });
                if (data == null) return false;
                var reqData = _dataAdditionalDiffField!.GetValue(data);
                if (reqData == null) return false;
                requirements = (string[])_dataRequirementsField!.GetValue(reqData);
                return true;
            } catch (Exception e) {
                Plugin.Log.Error($"GetRequirements failed: \r\n {e}");
                return false;
            }
        }

        #endregion

        #region TryGetCapabilities

        public static bool TryGetCapabilities(out IReadOnlyCollection<string>? capabilities) {
            capabilities = null;
            if (!isInitialized) return false;
            try {
                capabilities = (IReadOnlyCollection<string>)_collectionsGetCapabilitiesMethod!.Invoke(null, null);
                return true;
            } catch (Exception e) {
                Plugin.Log.Error($"GetCapabilities failed: \r\n {e}");
                return false;
            }
        }

        #endregion

        #region ValidateRequirements

        public static bool ValidateRequirements(IDifficultyBeatmap beatmap) {
            return !TryGetBeatmapRequirements(beatmap, out var requirements)
                || !TryGetCapabilities(out var capabilities)
                || (requirements?.All(x => capabilities?.Contains(x) ?? false) ?? true);
        }

        #endregion
    }
}