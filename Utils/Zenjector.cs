using System;
using System.Collections.Generic;
using Zenject;
using Zenject.Internal;

namespace BeatLeader.Utils
{
    public class Zenjector
    {
        public enum Location
        {
            GameplayCore,
            Menu,
            App
        }
        public enum Time
        {
            Before,
            After
        }

        public static DiContainer gameplayCoreContainer;
        public static DiContainer menuContainer;
        public static DiContainer appContainer;

        public static event Action<DiContainer> beforeGameplayCoreInstalled;
        public static event Action<DiContainer> beforeMenuInstalled;
        public static event Action<DiContainer> beforeAppInstalled;

        public static event Action<DiContainer> afterGameplayCoreInstalled;
        public static event Action<DiContainer> afterMenuInstalled;
        public static event Action<DiContainer> afterAppInstalled;

        public static void InvokeEvent(Location location, Time time)
        {
            switch (location)
            {
                case Location.GameplayCore:
                    if (gameplayCoreContainer == null) return;
                    if (time == Time.Before)
                        beforeGameplayCoreInstalled?.Invoke(gameplayCoreContainer);
                    else
                        afterGameplayCoreInstalled?.Invoke(gameplayCoreContainer);
                    return;
                case Location.Menu:
                    if (menuContainer == null) return;
                    if (time == Time.Before)
                        beforeMenuInstalled?.Invoke(menuContainer);
                    else
                        afterMenuInstalled?.Invoke(menuContainer);
                    return;
                case Location.App:
                    if (appContainer == null) return;
                    if (time == Time.Before)
                        beforeAppInstalled?.Invoke(appContainer);
                    else
                        afterAppInstalled?.Invoke(appContainer);
                    return;
            }
        }
        public static void InvokeEvent(Location location, Time time, DiContainer container)
        {
            switch (location)
            {
                case Location.GameplayCore:
                    if (time == Time.Before)
                        beforeGameplayCoreInstalled?.Invoke(container);
                    else
                        afterGameplayCoreInstalled?.Invoke(container);
                    return;
                case Location.Menu:
                    if (time == Time.Before)
                        beforeMenuInstalled?.Invoke(container);
                    else
                        afterMenuInstalled?.Invoke(container);
                    return;
                case Location.App:
                    if (time == Time.Before)
                        beforeAppInstalled?.Invoke(container);
                    else
                        afterAppInstalled?.Invoke(container);
                    return;
            }
        }
    }
}
