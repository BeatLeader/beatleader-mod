using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class PauseMenuSabersManager : MonoBehaviour
    {
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;

        protected GameObject _controllersContainer;
        protected VRController _leftController;
        protected VRController _rightController;

        public VRController leftController => _leftController;
        public VRController rightController => _rightController;

        public void Start()
        {
            _controllersContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            _leftController = _controllersContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            _rightController = _controllersContainer.transform.Find("ControllerRight").GetComponent<VRController>();
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
