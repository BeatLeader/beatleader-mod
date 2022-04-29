using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplayPlaybackUI : MonoBehaviour
    {
        [Inject] protected readonly PauseMenuSabersManager _menuSabersManager;

        protected FloatingScreen _floatingScreen;
        protected bool _initialized;

        public void Start()
        {
            if (!_initialized)
            {
                _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(20, 50), false, Vector3.zero, Quaternion.identity);
                BSMLParser.instance.Parse(Utilities.GetResourceContent(
                    Assembly.GetExecutingAssembly(), Plugin.ResourcesPath + ".BSML.ReplayPlaybackUI.bsml"), _floatingScreen.gameObject, this);
                _floatingScreen.transform.SetParent(_menuSabersManager.leftController.transform, false);
                _floatingScreen.transform.localPosition = new Vector2(0.55f, 0);
                _floatingScreen.transform.localEulerAngles = new Vector3(90, 90, 0);
                _initialized = true;
            }
        }
    }
}
