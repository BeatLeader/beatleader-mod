using UnityEngine;

namespace BeatLeader.Models {
    public struct SerializableColor {
        public SerializableColor(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public SerializableColor(float r, float g, float b) {
            this.r = r;
            this.g = g;
            this.b = b;
            a = 1f;
        }

        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(SerializableColor color) {
            return new(color.r, color.g, color.b, color.a);
        }
        
        public static implicit operator SerializableColor(Color color) {
            return new(color.r, color.g, color.b, color.a);
        }
    }
}