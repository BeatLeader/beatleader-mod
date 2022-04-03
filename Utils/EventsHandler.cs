using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatLeader.Utils
{
    internal class EventsHandler
    {
        private static GameScenesManager gameScenesManager;

        public static event Action GameSceneLoaded;
        public static event Action MenuSceneLoadedFresh;
        public static event Action MenuSceneLoaded;

        private static bool lastSceneWasNotMenu;
        private static string[] scenesForFresh = { "GameCore", "Credits", "BeatmapEditor" };

        public static void Init()
        {
            SceneManager.activeSceneChanged += CompareEvents;
            gameScenesManager = gameScenesManager == null ? Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault() : null;
        }
        private static void CompareEvents(Scene unloadedScene, Scene loadedScene)
        {
            if (loadedScene.name == "GameCore")
            {
                gameScenesManager.transitionDidFinishEvent += InvokeGameSceneLoadedEvent;
            }
            else if (loadedScene.name == "MainMenu")
            {
                if (unloadedScene.name == "EmptyTransition" && !lastSceneWasNotMenu)
                {
                    gameScenesManager.transitionDidFinishEvent += InvokeMenuSceneLoadedFreshEvent;
                }
                MenuSceneLoaded?.Invoke();
                lastSceneWasNotMenu = false;
            }
            if (scenesForFresh.Contains(loadedScene.name))
                lastSceneWasNotMenu = true;
        }
        private static void InvokeMenuSceneLoadedFreshEvent(ScenesTransitionSetupDataSO scenesTransitionSetupData, DiContainer di)
        {
            MenuSceneLoadedFresh?.Invoke();
            gameScenesManager.transitionDidFinishEvent -= InvokeMenuSceneLoadedFreshEvent;
        }
        private static void InvokeGameSceneLoadedEvent(ScenesTransitionSetupDataSO scenesTransitionSetupData, DiContainer di)
        {
            GameSceneLoaded?.Invoke();
            gameScenesManager.transitionDidFinishEvent -= InvokeGameSceneLoadedEvent;
        }
    }
}
