using UVector2 = UnityEngine.Vector2;

namespace BeatLeader.Models {
    internal struct SerializableVector2 {
        public SerializableVector2(UVector2 unityVector) {
            x = unityVector.x;
            y = unityVector.y;
        }
        public SerializableVector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public static implicit operator UVector2(SerializableVector2 vector) => new UVector2(vector.x, vector.y);
        public static implicit operator SerializableVector2(UVector2 vector) => new SerializableVector2(vector);

        public float x;
        public float y;
    }
}
