using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.XR;
using UnityEngine;
using BeatLeader.Replays.Emulators;

namespace BeatLeader.Replays
{
    public class ReplaySabersHelper
    {
        public static ReplayVRController CreateFakeReplaySaber(SaberType saberType)
        {
            switch (saberType)
            {
                case SaberType.SaberA:
                    {
                        var saber = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.gameObject.name == "LeftHand");
                        if (saber != null)
                        {
                            var replaySaber = GameObject.Instantiate(saber);
                            replaySaber.name = "LeftReplaySaber";
                            GameObject.Destroy(replaySaber.GetComponent<VRController>());
                            var replayVRController = replaySaber.gameObject.AddComponent<ReplayVRController>();
                            replayVRController.node = XRNode.LeftHand;
                            return replayVRController;
                        }
                    }
                    break;
                case SaberType.SaberB:
                    {
                        var saber = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.gameObject.name == "RightHand");
                        if (saber != null)
                        {
                            var replaySaber = GameObject.Instantiate(saber);
                            replaySaber.name = "RightReplaySaber";
                            GameObject.Destroy(replaySaber.GetComponent<VRController>());
                            var replayVRController = replaySaber.gameObject.AddComponent<ReplayVRController>();
                            replayVRController.node = XRNode.RightHand;
                            return replayVRController;
                        }
                    }
                    break;
            }
            return null;
        }
    }
}
