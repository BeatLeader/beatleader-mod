using System;
using BeatLeader.Installers;
using BeatLeader;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using BeatSaberMarkupLanguage;
using System.Reflection;

namespace BeatLeader
{
    [HarmonyPatch(typeof(PauseMenuManager), "Start")]
    public static class ReplayPauseMenuPatch
    {
        [UsedImplicitly]
        
        private static void Postfix(PauseMenuManager __instance)
        {
            GameObject replayer = GameObject.Find("Replayer");
            if (replayer == null)
                return;
            GameObject canvas = __instance.transform.Find("Wrapper/MenuWrapper/Canvas").gameObject;
            GameObject mainBar = canvas.transform.Find("MainBar").gameObject;
            mainBar.SetActive(false);
            var controller = canvas.AddComponent<ReplayPauseViewController>();
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), Plugin.ResourcesPath + ".BSML.ReplayPauseView.bsml"), canvas, controller);
        }
    }
}