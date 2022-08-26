using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replayer.PluginsTools
{
    public class NoodleExtensionsPluginReflector : PluginAssemblyReflector
    {
        protected override string PluginName => "NoodleExtensions";

        private Type _beatmapCallbacksUpdaterType;
        private FieldInfo _prevSongTimeInfo;

        protected override void OnLoad()
        {
            _beatmapCallbacksUpdaterType = GetPluginType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager");
            _prevSongTimeInfo = ReflectField(_beatmapCallbacksUpdaterType, "_prevSongTime");
        }
    }
}
