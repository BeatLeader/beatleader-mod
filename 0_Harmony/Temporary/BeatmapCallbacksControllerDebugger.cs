using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace BeatLeader
{
    //[HarmonyPatch()]
    public class BeatmapCallbacksControllerDebugger
    {
        static MethodInfo TargetMethod()
        {
            foreach (MethodInfo m in typeof(BeatmapCallbacksController).GetMethods())
            {
                Plugin.Log.Info($"{m.Name}");
                var cond = (m.Name == "AddBeatmapCallback"
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].Name == "aheadTime"
                    && m.GetParameters()[1].Name == "callback");
                Plugin.Log.Info($"{cond}");
                if (cond) return m;
            }
            return null;
        }

        static void Postfix()
        {
            Debug.LogWarning("Adding");
        }
    }
}
