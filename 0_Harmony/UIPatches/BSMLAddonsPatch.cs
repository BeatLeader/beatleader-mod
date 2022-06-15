using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.UI.BSML_Addons;
using BeatSaberMarkupLanguage;
using UnityEngine;
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
