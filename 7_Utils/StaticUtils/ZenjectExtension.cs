using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Utils
{
    public static class ZenjectExtension
    {
        public static void Reinject<T>(this DiContainer container)
        {
            if (container == null) return;
            foreach (var item in container.AllContracts)
            {
                if (item.Type.ContainsFieldOfTypeMarkedWithAttribute(typeof(T), typeof(InjectAttribute)))
                {
                    container.Inject(item);
                }
            }
        }
        public static T InstantiateComponentOnNewGameObjectSelf<T>(this DiContainer container) where T : Component
        {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            return component;
        }
    }
}
