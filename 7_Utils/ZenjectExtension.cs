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
        public static void InjectAllFieldsOfTypeOnFindedGameObjects<T>(List<Type> objects, DiContainer container)
        {
            foreach (var item in objects)
            {
                var instance = Resources.FindObjectsOfTypeAll(item).FirstOrDefault();
                if (instance != null)
                {
                    container.Inject(instance);
                }
            }
        }
    }
}
