using UnityEngine;

namespace BeatLeader {
    public class ChristmasTreeLevel : MonoBehaviour {
        public float topRadius;
        public float topHeight;
        public float bottomRadius;
        public float bottomHeight;

        // thanks, ChatGPT
        public void Draw(float factor) {
            const int segments = 32; // Number of segments to approximate the circles
            const float angleStep = 360f / segments;

            var topHeight = this.topHeight * factor;
            var bottomHeight = this.bottomHeight * factor;
            var topRadius = this.topRadius * factor;
            var bottomRadius = this.bottomRadius * factor;

            var topCenter = transform.position + Vector3.up * topHeight;
            var bottomCenter = transform.position + Vector3.up * bottomHeight;

            // Draw the top circle in red
            Gizmos.color = Color.red;
            DrawCircle(topCenter, topRadius, segments);

            // Draw the bottom circle in blue
            Gizmos.color = Color.blue;
            DrawCircle(bottomCenter, bottomRadius, segments);

            // Draw the sides of the truncated cone
            Gizmos.color = Color.white;
            var previousTopPoint = topCenter + Vector3.right * topRadius;
            var previousBottomPoint = bottomCenter + Vector3.right * bottomRadius;

            for (var i = 1; i <= segments; i++) {
                var angle = angleStep * i * Mathf.Deg2Rad;

                // Calculate the points for this segment
                var newTopPoint = topCenter + new Vector3(Mathf.Cos(angle) * topRadius, 0, Mathf.Sin(angle) * topRadius);
                var newBottomPoint = bottomCenter + new Vector3(Mathf.Cos(angle) * bottomRadius, 0, Mathf.Sin(angle) * bottomRadius);

                // Connect the previous points to the new points
                Gizmos.DrawLine(previousTopPoint, newTopPoint);         // Top circle
                Gizmos.DrawLine(previousBottomPoint, newBottomPoint);   // Bottom circle
                Gizmos.DrawLine(previousTopPoint, previousBottomPoint); // Side line

                // Update the previous points
                previousTopPoint = newTopPoint;
                previousBottomPoint = newBottomPoint;
            }
        }

        private static void DrawCircle(Vector3 center, float radius, int segments) {
            var angleStep = 360f / segments;
            var previousPoint = center + Vector3.right * radius;

            for (var i = 1; i <= segments; i++) {
                var angle = angleStep * i * Mathf.Deg2Rad;
                var newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                // Draw the circle segment
                Gizmos.DrawLine(previousPoint, newPoint);

                previousPoint = newPoint;
            }
        }
    }
}