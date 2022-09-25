using BeatLeader.Utils;
using IPA.Loader;
using System;
using System.Reflection;

namespace BeatLeader.Interop
{
    internal static class ScoreSaberInterop
    {
        private static Assembly _assembly;
        private static Type _installerType1;
        private static Type _installerType2;
        private static Type _installerType3;
        private static MethodInfo _installMethodInfo1;
        private static MethodInfo _installMethodInfo2;
        private static MethodInfo _installMethodInfo3;
        private static HarmonyMultisilencer _installatorsSilencer;

        public static bool RecordingEnabled
        {
            get => !_installatorsSilencer?.Enabled ?? true;
            set
            {
                if (_installatorsSilencer != null)
                    _installatorsSilencer.Enabled = !value;
            }
        }

        //UAHAHAHAH, DIE SOSABER
        public static void Init()
        {
            _assembly = PluginManager.GetPluginFromId("ScoreSaber")?.Assembly;
            if (_assembly == null) return;

            try
            {
                _installerType1 = _assembly.GetType("#=zX_v74G3PzI7aCZIakETp3Jix2zF4LcMlbuBZjFrpZV7rLX_AyQ==");
                _installerType2 = _assembly.GetType("#=z04iHj80ruN9mJYvYhK1kO8nt_cId_4J1Mi_w_iWR2Ltr");
                _installerType3 = _assembly.GetType("#=zSiVtl_ZozGZMq$Qlhd4LwcTOeQbdl3_dta0EEVJIT8xJ");
                _installMethodInfo1 = _installerType1.GetMethod("InstallBindings", ReflectionUtils.DefaultFlags);
                _installMethodInfo2 = _installerType2.GetMethod("InstallBindings", ReflectionUtils.DefaultFlags);
                _installMethodInfo3 = _installerType3.GetMethod("InstallBindings", ReflectionUtils.DefaultFlags);
                _installatorsSilencer = new(new[] { _installMethodInfo1, _installMethodInfo2, _installMethodInfo3 }, false);
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve ScoreSaber data, replays system may conflict with ScoreSaber!");
            }
        }
    }
}
