using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Installers;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.Emulators;
using BeatLeader.UI;
using BeatLeader.Utils;
using Zenject;
using BeatLeader.Utils.Expansions;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class MovementPatchHelper
    {
        public static ReplayPlayer player;
        public static ReplayVRController leftReplayController => player.leftHand;
        public static ReplayVRController rightReplayController => player.rightHand;

        public static void InstallPatch()
        {
            VRController rightHand = null;
            VRController leftHand = null;

            foreach (var controller in Resources.FindObjectsOfTypeAll<VRController>())
            {
                switch (controller.gameObject.name)
                {
                    case "LeftHand":
                        leftHand = controller;
                        break;
                    case "RightHand":
                        rightHand = controller;
                        break;
                }
            }
            if (rightHand == null & leftHand == null) return;

            var leftGo = leftHand.gameObject;
            var rightGo = rightHand.gameObject;

            GameObject.Destroy(leftHand);
            GameObject.Destroy(rightHand);

            var leftReplayController = leftGo.AddComponent<ReplayVRController>();
            var rightReplayController = rightGo.AddComponent<ReplayVRController>();

            leftReplayController.node = UnityEngine.XR.XRNode.LeftHand;
            rightReplayController.node = UnityEngine.XR.XRNode.RightHand;

            player = new GameObject("ReplayPlayer").AddComponent<ReplayPlayer>();
            player.leftHand = leftReplayController;
            player.rightHand = rightReplayController;
            //player.replayData = ReplayUI.replayData;
            //CreateFakeSabers(out VRController leftHand, out VRController rightHand);
        }
        public static void UninstallPatch()
        {
            Plugin.Log.Warn("Uninstalling replay patch");
            ReplayUI.NotifyReplayEnded();
            player = null;
            EventsHandler.MenuSceneLoaded -= UninstallPatch;
        }
        public static void CreateFakeSabers(out VRController leftHand, out VRController rightHand)
        {
            leftHand = null;
            rightHand = null;
            foreach (var controller in Resources.FindObjectsOfTypeAll<VRController>())
            {
                switch (controller.gameObject.name)
                {
                    case "LeftHand":
                        leftHand = GameObject.Instantiate(controller);
                        break;
                    case "RightHand":
                        rightHand = GameObject.Instantiate(controller);
                        break;
                    default:
                        continue;
                }
            }
        }
    }
}
