using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BeatLeader.Replays;
using HarmonyLib;
using IPA.Utilities;

//[HarmonyPatch(typeof(BoxCuttableBySaber), "Cut")]
internal class ColliderDebugger
{
    private static void Postfix(Vector3 cutPoint, Quaternion orientation, Vector3 cutDirVec)
    {
        Debug.LogWarning("Cutted");
        Debug.LogWarning($"{cutPoint.x}/{cutPoint.y}/{cutPoint.z}");
        Debug.LogWarning($"{orientation.x}/{orientation.y}/{orientation.z}/{orientation.w}");
        Debug.LogWarning($"{cutDirVec.x}/{cutDirVec.y}/{cutDirVec.z}");
    }
}

//[HarmonyPatch(typeof(BeatmapObjectManager), "AddSpawnedNoteController")]
internal class NoteSpawnMethodDebugger
{
    private static ReplayPlayer currentPlayer => Resources.FindObjectsOfTypeAll<ReplayPlayer>().FirstOrDefault();
    private static void Postfix(NoteController noteController)
    {
        Debug.LogWarning($"NoteWasSpawned with {noteController.noteData.time}");
        //currentPlayer.CutNote(noteController);
    }
}

//[HarmonyPatch(typeof(CuttableBySaber), "CallWasCutBySaberEvent")]
internal class NoteCutMethodDebugger
{
    private static void Postfix(CuttableBySaber __instance)
    {
        if (__instance == null) return;
        var delegates = (Delegate[])__instance.GetType().GetField("wasCutBySaberEvent").GetValue(__instance);
        if (delegates != null)
            foreach (var line in delegates)
            {
                Debug.LogWarning(line);
            }
    }
}

//[HarmonyPatch(typeof(SaberMovementData), "ComputeSwingRating")]
internal class SaberMovementDataPatch
{
    private static void Postfix(SaberMovementData __instance)
    {

    }
}

//[HarmonyPatch(typeof(CutScoreBuffer), "Init")]
internal class CutScoreBufferPatch //списки с ивентами пустые, кат баффер нам не нужон
{
    private static void Postfix(CutScoreBuffer __instance)
    {
        var field1 = __instance.GetField<LazyCopyHashSet<ICutScoreBufferDidChangeReceiver>, CutScoreBuffer>("_didChangeEvent");
        var field2 = __instance.GetField<LazyCopyHashSet<ICutScoreBufferDidFinishReceiver>, CutScoreBuffer>("_didFinishEvent");
        Debug.LogWarning(field1.items.Count);
        Debug.LogWarning(field2.items.Count);
        foreach (var item in field1.items)
        {
            Debug.LogWarning(item);
        }
        foreach (var item in field2.items)
        {
            Debug.LogWarning(item);
        }
    }
}