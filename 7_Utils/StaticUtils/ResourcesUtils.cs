using BeatSaberMarkupLanguage.Animations;
using System.IO;
using System.Reflection;
using System;
using System.Threading.Tasks;
using IPA.Utilities.Async;
using UnityEngine.UI;
using UnityEngine;
using static BeatSaberMarkupLanguage.BeatSaberUI;
using BeatSaberMarkupLanguage;
using System.Drawing;

namespace BeatLeader {
    public static class ResourcesUtils 
    {
        public static Stream GetEmbeddedResourceStream(string resourceName) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }
        public static string[] GetEmbeddedResourceNames() {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
    }
}