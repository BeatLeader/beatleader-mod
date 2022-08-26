using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IPA.Loader;
using UnityEngine;

namespace BeatLeader.Replayer.PluginsTools
{
    public abstract class PluginAssemblyReflector
    {
        public PluginAssemblyReflector() { Load(); }

        protected abstract string PluginName { get; }
        protected Assembly PluginAssembly { get; private set; }
        public bool Detected { get; private set; }

        protected void Load()
        {
            var pluginMetadata = PluginManager.GetPluginFromId(PluginName);
            if (pluginMetadata == null) return;

            PluginAssembly = pluginMetadata.Assembly;
            Detected = true;

            OnLoad();
        }
        protected virtual void OnLoad()
        {

        }

        protected Type GetPluginType(string name)
        {
            return PluginAssembly.GetType(name);
        }
        protected FieldInfo ReflectField(Type type, string name, BindingFlags flags =
             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            return type.GetField(name, flags);
        }
        protected PropertyInfo ReflectProperty(Type type, string name, BindingFlags flags =
             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            return type.GetProperty(name, flags);
        }
    }
}
