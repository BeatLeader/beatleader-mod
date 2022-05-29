using System;
using System.Linq;
using System.Reflection;
using SiraUtil.Tools.FPFC;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class SceneTweaksManager : MonoBehaviour
    {
        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly SoftLocksController _softLocksController;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;

        public void Start()
        {
            ApplyTweaks();
        }
        public void ApplyTweaks()
        {
            //_softLocksController.InstallLock(_pauseMenuManager, SoftLocksController.LockMode.WhenRequired);
            if (_initData.noteSyncMode)
            {
                Resources.FindObjectsOfTypeAll<CuttingManager>().FirstOrDefault().enabled = false;
            }
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
