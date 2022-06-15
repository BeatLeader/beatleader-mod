using System.IO;
using System.Reflection;

namespace BeatLeader {
    public static class ResourcesUtils {
        public static Stream GetEmbeddedResourceStream(string resourceName) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        public static string[] GetEmbeddedResourceNames() {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
    }
}