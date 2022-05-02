using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class MenuSabersManager : MonoBehaviour
    {
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;

        protected GameObject _controllersContainer;
        protected VRController _leftController;
        protected VRController _rightController;
        protected bool _initialized;

        public VRController leftController => _leftController;
        public VRController rightController => _rightController;
        public bool initialized => _initialized;

        public void Start()
        {
            _controllersContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            _leftController = _controllersContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            _rightController = _controllersContainer.transform.Find("ControllerRight").GetComponent<VRController>();
            _initialized = true;
        }
        public void ShowMenuControllers()
        {
            _controllersContainer.SetActive(true);
        }
        public void HideMenuControllers()
        {
            _controllersContainer.SetActive(false);
        }
    }
}
