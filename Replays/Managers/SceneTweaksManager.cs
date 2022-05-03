using System;
using System.Collections.Generic;
using System.Linq;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class SceneTweaksManager : MonoBehaviour
    {
        [Inject] protected readonly InputManager _inputManager;

        public void Start()
        {
            ApplyTweaks();
        }
        public void ApplyTweaks()
        {
            if (_inputManager.currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                Resources.FindObjectsOfTypeAll<VRLaserPointer>().First().gameObject.SetActive(false);
                Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().First().gameObject.SetActive(false);
            }
            else if(_inputManager.currentInputSystem == InputManager.InputSystemType.VR)
            {
                Resources.FindObjectsOfTypeAll<VRLaserPointer>().First().gameObject.SetActive(true);
                Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().First().gameObject.SetActive(true);
            }
        }
    }
}
