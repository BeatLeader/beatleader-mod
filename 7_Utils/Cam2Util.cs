using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    internal static class Cam2Util
    {
        public static bool Detected { get; private set; } = false;

        public static void Init()
        {
            var pluginInfo = PluginManager.GetPluginFromId("Camera2");
            if (pluginInfo == null) return;
            Detected = true;
        }
    }
}
