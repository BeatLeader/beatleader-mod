using BeatLeader.Attributes;
using BeatLeader.Utils;
using System;
using System.Reflection;

namespace BeatLeader.Interop {
    [PluginInterop("Custom Notes")]
    internal static class CustomNotesInterop {
        #region Init

        [PluginType("CustomNotes.Managers.CustomNoteController")]
        private static readonly Type _customNoteControllerType;

        [PluginType("CustomNotes.Managers.CustomBombController")]
        private static readonly Type _customBombControllerType;

        [PluginType("CustomNotes.Managers.CustomBurstSliderController")]
        private static readonly Type _customSliderControllerType;

        [PluginState]
        private static readonly bool _isInitialized;

        private static MethodInfo _missNoteControllerMethod;
        private static MethodInfo _missSliderControllerMethod;
        private static MethodInfo _finishBombControllerMethod;
        private static FieldInfo _bombControllerContainerField;

        [InteropEntry]
        private static void Init() {
            _missNoteControllerMethod = _customNoteControllerType
                .GetMethod("HandleNoteControllerNoteWasMissed", ReflectionUtils.DefaultFlags);
            _missSliderControllerMethod = _customSliderControllerType
                .GetMethod("HandleNoteControllerNoteWasMissed", ReflectionUtils.DefaultFlags);
            _finishBombControllerMethod = _customBombControllerType
                .GetMethod("DidFinish", ReflectionUtils.DefaultFlags);
            _bombControllerContainerField = _customBombControllerType
                .GetField("container", ReflectionUtils.DefaultFlags);
        }

        #endregion

        #region Despawn

        public static bool TryDespawnCustomObject(NoteController controller) {
            if (!_isInitialized) return false;
            try {
                if (controller as GameNoteController != null)
                    DespawnCustomNote(controller);
                else if (controller as BombNoteController != null)
                    DespawnCustomBomb(controller);
                else if (controller as BurstSliderGameNoteController != null)
                    DespawnCustomSlider(controller);
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to despawn custom note \r\n" + ex);
                return false;
            }
        }

        private static void DespawnCustomNote(NoteController controller) {
            var component = controller.GetComponent(_customNoteControllerType);
            if (component == null) return;
            _missNoteControllerMethod.Invoke(component, new object[] { controller });
        }
        private static void DespawnCustomBomb(NoteController controller) {
            var component = controller.GetComponent(_customBombControllerType);
            if (component == null || _bombControllerContainerField.GetValue(component) == null) return;
            _finishBombControllerMethod.Invoke(component, null);
        }
        private static void DespawnCustomSlider(NoteController controller) {
            var component = controller.GetComponent(_customSliderControllerType);
            if (component == null) return;
            _missSliderControllerMethod.Invoke(component, new object[] { null });
        }

        #endregion
    }
}
