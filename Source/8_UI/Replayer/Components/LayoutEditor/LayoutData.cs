using System.Runtime.InteropServices;
using BeatLeader.Models;

namespace BeatLeader.Components {
    [StructLayout(LayoutKind.Auto)]
    public struct LayoutData {
        public SerializableVector2 position;
        public SerializableVector2 size;
        public int layer;
        public bool visible;
    }
}