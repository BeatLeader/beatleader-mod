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
        //ye, i know about rebind
        public static void Reinject<T>(this DiContainer container)
        {
            container.Reinject(container.Resolve<T>());
        }
        public static void Reinject<T>(this DiContainer container, T value)
        {
            if (container == null || value == null) return;
            foreach (var item in container.AllContracts)
                foreach (var item2 in item.Type.GetFields(x => x.FieldType == typeof(T)
                && x.GetCustomAttribute(typeof(InjectAttribute)) != null)) //MOOOORE COOODE IN OOOOONE LIIIINE
                    item2.SetValue(container.Resolve(item), value); 
        }
        public static T InstantiateComponentOnNewGameObjectSelf<T>(this DiContainer container) where T : Component
        {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            return component;
        }
    }
}
