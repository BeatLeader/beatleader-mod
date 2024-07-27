using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using IPA.Utilities;

namespace BeatLeader {
    internal static class DynamicLibLoader {
        public const string LibsPath = Plugin.ResourcesPath + ".Libs";

        private static readonly Dictionary<string, IntPtr> loadedLibPtrs = new();
        private static readonly string libsTempPath = Path.Combine(Path.GetTempPath(), "BeatLeaderDynamicLibraries");
        private static bool _loaded;

        public static void Load() {
            if (_loaded) return;
            var libs = ResourcesUtils
                .GetEmbeddedResourceNames()
                .Where(x => x.StartsWith(LibsPath));
            CreateTempDirectory();
            
            Plugin.Log.Debug("[LibLoader] Processing dynamic libraries:");
            foreach (var lib in libs) {
                //building the path
                var libName = GetResourceName(lib);
                var libPath = Path.Combine(libsTempPath, libName);
                //some logs
                Plugin.Log.Debug($"-> Processing lib {libName}:");
                Plugin.Log.Debug($"Extracting to {libPath}...");
                //writing library to the cache
                using (var stream = ResourcesUtils.GetEmbeddedResourceStream(lib)) {
                    using (var fileStream = new FileStream(libPath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                        stream.CopyTo(fileStream);
                    }
                }
                //loading
                Plugin.Log.Debug("Loading...");
                if (LoadLibrary(libPath) is var libPtr && libPtr != IntPtr.Zero) {
                    //adding if succeed
                    loadedLibPtrs.Add(libName, libPtr);
                    Plugin.Log.Debug($"{libName} has loaded successfully");
                } else {
                    Plugin.Log.Critical($"{libName} has failed to load");
                }
            }
            Plugin.Log.Debug("[LibLoader] Dynamic library loading finished");

            _loaded = true;
        }

        public static void Unload() {
            Plugin.Log.Debug("[LibLoader] Unloading dynamic libraries:");
            foreach (var (libName, ptr) in loadedLibPtrs) {
                Plugin.Log.Debug($"-> Unloading lib {libName}:");
                //unloading from the domain
                if (FreeLibrary(ptr)) {
                    Plugin.Log.Debug($"{libName} has unloaded successfully");
                } else {
                    Plugin.Log.Critical($"{libName} has failed to unload");
                }
            }
            
            // commented since FreeLibrary needs some time to finish its work,
            // so we cannot remove the cache until it release the file
            
            //removing the cache
            //Plugin.Log.Debug("[LibLoader] Removing the cached files...");
            //Directory.Delete(libsTempPath, true);
            Plugin.Log.Debug("[LibLoader] Dynamic library unloading finished");
        }

        private static void CreateTempDirectory() {
            if (!Directory.Exists(libsTempPath)) {
                Directory.CreateDirectory(libsTempPath);
            }
        }

        private static string GetResourceName(string path) {
            var lastIndex = path.LastIndexOf('.');
            var beforeLastIndex = path.LastIndexOf('.', lastIndex - 1);
            var name = path.Remove(0, beforeLastIndex + 1);
            return name;
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}