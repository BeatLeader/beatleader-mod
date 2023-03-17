using UnityEngine;

namespace BeatLeader {
    public class GraphPoint {
        public Vector2 Position;
        public Vector2 Tangent;
        
        public GraphPoint(Vector2 position, Vector2 tangent) {
            Position = position;
            Tangent = tangent;
        }
    }
}