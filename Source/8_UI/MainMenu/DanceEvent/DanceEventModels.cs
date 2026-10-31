using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeatLeader.UI.MainMenu {
    internal static class DanceEventModels {
        #region Models

        public class Model {
            public Mesh Mesh = null!;
            public string[] Materials = null!;

            public int IndexOf(string material) => System.Array.IndexOf(Materials, material);
        }

        private static readonly Dictionary<string, Model?> models = new();

        public static Model? Get(string file) {
            if (models.TryGetValue(file, out var model)) return model;
            model = Load(file);
            models[file] = model;
            return model;
        }

        private static Model? Load(string file) {
            using var stream = ResourcesUtils.GetEmbeddedResourceStream($"{Plugin.ResourcesPath}.Halloween.{file}");
            if (stream == null) return null;
            using var reader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress));
            return Parse(reader, file);
        }

        //OBJ is right-handed: x is mirrored and the winding flipped, as Unity's own OBJ import does
        private static Model Parse(TextReader reader, string name) {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var vertices = new List<Vector3>();
            var vertexNormals = new List<Vector3>();
            var vertexUvs = new List<Vector2>();
            var lookup = new Dictionary<(int, int, int), int>();
            var groups = new List<(string material, List<int> indices)>();
            List<int>? current = null;
            var corners = new List<int>();

            string? line;
            while ((line = reader.ReadLine()) != null) {
                var parts = line.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                switch (parts[0]) {
                    case "v":
                        positions.Add(MirroredVector(parts));
                        break;
                    case "vn":
                        normals.Add(MirroredVector(parts));
                        break;
                    case "vt":
                        uvs.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                        break;
                    case "usemtl":
                        var material = parts.Length > 1 ? parts[1] : "";
                        current = groups.FirstOrDefault(x => x.material == material).indices;
                        if (current == null) {
                            current = new List<int>();
                            groups.Add((material, current));
                        }
                        break;
                    case "f":
                        if (current == null) {
                            current = new List<int>();
                            groups.Add(("", current));
                        }
                        corners.Clear();
                        for (var i = 1; i < parts.Length; i++) {
                            corners.Add(Vertex(parts[i]));
                        }
                        for (var i = 1; i + 1 < corners.Count; i++) {
                            current.Add(corners[0]);
                            current.Add(corners[i + 1]);
                            current.Add(corners[i]);
                        }
                        break;
                }
            }

            int Vertex(string corner) {
                var ids = corner.Split('/');
                var p = ZeroBasedIndex(ids[0], positions.Count);
                var t = ids.Length > 1 && ids[1].Length > 0 ? ZeroBasedIndex(ids[1], uvs.Count) : -1;
                var n = ids.Length > 2 && ids[2].Length > 0 ? ZeroBasedIndex(ids[2], normals.Count) : -1;
                if (lookup.TryGetValue((p, t, n), out var index)) return index;
                index = vertices.Count;
                vertices.Add(positions[p]);
                vertexUvs.Add(t >= 0 ? uvs[t] : Vector2.zero);
                vertexNormals.Add(n >= 0 ? normals[n] : Vector3.up);
                lookup[(p, t, n)] = index;
                return index;
            }

            var mesh = new Mesh { name = name };
            if (vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(vertexNormals);
            mesh.SetUVs(0, vertexUvs);
            mesh.subMeshCount = groups.Count;
            for (var i = 0; i < groups.Count; i++) {
                mesh.SetTriangles(groups[i].indices, i);
            }
            mesh.RecalculateBounds();
            return new Model { Mesh = mesh, Materials = groups.Select(x => x.material).ToArray() };
        }

        private static Vector3 MirroredVector(string[] parts) {
            return new Vector3(-ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3]));
        }

        private static float ParseFloat(string value) {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static int ZeroBasedIndex(string objIndex, int count) {
            var index = int.Parse(objIndex, CultureInfo.InvariantCulture);
            return index > 0 ? index - 1 : count + index;
        }

        #endregion

        #region Materials

        private static Material? _noteMenuMaterial;
        private static Material? _cursorMaterial;

        private static readonly int ambientMinimumId = Shader.PropertyToID("_AmbientMinimalValue");

        public static Material? Solid(Color color, float minimumLight) {
            var material = Copy(ref _noteMenuMaterial, "NoteMenu", color);
            if (material == null) return null;
            material.SetFloat(ambientMinimumId, minimumLight);
            return material;
        }

        public static Material? Glowing(Color color) => Copy(ref _cursorMaterial, "UICursor", color);

        private static Material? Copy(ref Material? source, string name, Color color) {
            if (source == null) {
                source = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(x => x.name == name);
                if (source == null) return null;
            }
            return new Material(source) { name = $"DanceEvent{name}", color = color };
        }

        public static Color Hex(string hex) {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }

        #endregion
    }
}
