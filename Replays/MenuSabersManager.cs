using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class MenuSabersManager : MonoBehaviour
    {
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;

        private GameObject controllersContainer;
        private VRController leftController;
        private VRController rightController;

        public float transitionTime;

        public void Start()
        {
            controllersContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            leftController = controllersContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            rightController = controllersContainer.transform.Find("ControllerRight").GetComponent<VRController>();
        }
        public void ShowMenuControllers()
        {
            controllersContainer.SetActive(true);
        }
        public void HideMenuControllers()
        {
            controllersContainer.SetActive(false);
        }
    }
}
