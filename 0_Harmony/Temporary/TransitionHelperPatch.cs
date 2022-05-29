using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.UI;
using BeatmapEditor3D;
using HarmonyLib;
using UnityEngine;

namespace BeatLeader.Temporary
{
    [HarmonyPatch(typeof(BeatmapEditorScreenSystem), "Awake")]
    internal class TransitionHelperPatch
    {
        static void Postfix()
        {
           //Debug.LogWarning("Binding!");
           //new GameObject("PlaybackNonVRViewController").AddComponent<PlaybackNonVRViewController>().transform.SetParent(
           //    Resources.FindObjectsOfTypeAll<BeatmapEditorHierarchyManager>().First().transform.Find("ScreenContainer"));
        }
    }
}
