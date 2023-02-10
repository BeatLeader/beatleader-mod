using BeatLeader.Attributes;
using BeatLeader.Utils;
using System;
using System.Reflection;
using TMPro;

namespace BeatLeader.Interop {
    [PluginInterop("Counters+")]
    internal class CountersPlusInterop {
        [PluginType("CountersPlus.Counters.MissedCounter")]
        private static readonly Type _missedCounterType = null!;

        private static HarmonyPatchDescriptor _missedCounterInitDescriptor = null!;
        private static HarmonyAutoPatch _missedCounterInitPatch = null!;
        private static MethodInfo _missedCounterNoteCutMtd = null!;
        private static FieldInfo _missedCounterNotesMissedTextFld = null!;
        private static FieldInfo _missedCounterNotesMissedFld = null!;
        private static object? _missedCounterObj;

        [InteropEntry]
        private static void Init() {
            _missedCounterInitDescriptor = new(_missedCounterType
                .GetMethod("CounterInit", ReflectionUtils.DefaultFlags), postfix: 
                typeof(CountersPlusInterop).GetMethod(nameof(
                    MissedCounterInitPostfix), ReflectionUtils.StaticFlags));
            _missedCounterNotesMissedFld = _missedCounterType
                .GetField("notesMissed", ReflectionUtils.DefaultFlags);
            _missedCounterNotesMissedTextFld = _missedCounterType
                .GetField("counter", ReflectionUtils.DefaultFlags);
            _missedCounterNoteCutMtd = _missedCounterType
                .GetMethod("OnNoteCut", ReflectionUtils.DefaultFlags);
            _missedCounterInitPatch = new(_missedCounterInitDescriptor);
        }

        public static void ResetMissedCounter() {
            InteractMissedCounter(static x => {
                _missedCounterNotesMissedFld.SetValue(x, 0);
                var text = ((TMP_Text)_missedCounterNotesMissedTextFld
                    .GetValue(_missedCounterObj)).text = 0.ToString();
            });
        }

        public static void HandleMissedCounterNoteWasCut(NoteCutInfo cutInfo) {
            InteractMissedCounter(x => {
                _missedCounterNoteCutMtd.Invoke(x, 
                    new object[] { cutInfo.noteData, cutInfo });
            });
        }

        private static void InteractMissedCounter(Action<object> callback) {
            try {
                if (_missedCounterObj == null) {
                    Plugin.Log.Warn("MissedCounter instance is not captured! Is counter uninitialized?");
                    return;
                }
                callback(_missedCounterObj);
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to interact missed counter: " + ex);
            }
        }

        private static void MissedCounterInitPostfix(object __instance) {
            _missedCounterObj = __instance;
        }
    }
}
