using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    public class HotkeysHandler : IInitializable, ITickable
    {
        public readonly List<GameHotkey> hotkeys = new()
        {
            new HideUIHotkey(),
            new HideCursorHotkey(),
            new PauseHotkey(),
            new RewindBackwardHotkey(),
            new RewindForwardHotkey()
        };//not static to dispose the garbage inside hotkeys

        [Inject] private readonly DiContainer _container;

        public void Initialize()
        {
            foreach (var item in hotkeys)
            {
                _container.Inject(item);
            }
        }
        public void Tick()
        {
            foreach (var item in hotkeys)
            {
                try
                {
                    if (Input.GetKeyDown(item.Key))
                        item.OnKeyDown();

                    else if (Input.GetKeyUp(item.Key))
                        item.OnKeyUp();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"[HotkeysHandler] Error during attempting to perform {item.GetType().Name} hotkey! \r\n {ex}");
                }
            }
        }
    }
}
