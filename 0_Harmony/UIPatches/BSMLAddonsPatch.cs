using BeatLeader.Replays.UI.BSML_Addons;
using BeatSaberMarkupLanguage;
using HarmonyLib;

namespace BeatLeader
{
    [HarmonyPatch(typeof(BSMLParser), "Awake")]
    public static class BSMLAddonsPatch
    {
        private static void Postfix()
        {
            Plugin.Log.Info("Loading BeatLeader BSML addons");
            BSMLAddonsLoader.LoadAddons();
        }
    }
}
