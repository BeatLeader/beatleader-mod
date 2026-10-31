using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal static class DanceEventDecorSprites {
        #region Rendered

        public static Sprite? Pumpkin => Load("pumpkin.png", new Vector2(0.5f, 0.5f));
        public static Sprite? Spider => Load("spider.png", new Vector2(0.5f, 0.8f));
        public static Sprite? Web => Load("web.png", new Vector2(0f, 1f));
        public static Sprite? BatUp => Load("bat-up.png", new Vector2(0.5f, 0.5f));
        public static Sprite? BatDown => Load("bat-down.png", new Vector2(0.5f, 0.5f));

        public class Candle {
            public readonly string File;
            public readonly float Bottom;
            public readonly Vector2 Wick;

            public Candle(string file, float bottom, Vector2 wick) {
                File = file;
                Bottom = bottom;
                Wick = wick;
            }

            public Sprite? Sprite => Load(File, new Vector2(0.5f, Bottom));
        }

        public static readonly Vector2 CandleSpriteSize = new(0.5f, 1.25f);

        public static readonly Candle[] Candles = {
            new("candle-tall.png", 0.04f, new Vector2(0.51f, 0.875f)),
            new("candle-medium.png", 0.035f, new Vector2(0.5f, 0.655f)),
            new("candle-short.png", 0.0275f, new Vector2(0.5f, 0.45f))
        };

        private static readonly Dictionary<string, Sprite?> loaded = new();

        private static Sprite? Load(string file, Vector2 pivot) {
            if (loaded.TryGetValue(file, out var sprite)) return sprite;
            sprite = DanceDreadApparitions.LoadSprite(file, pivot);
            loaded[file] = sprite;
            return sprite;
        }

        #endregion

        #region Generated

        private static Sprite? _flame;
        private static Sprite? _glow;
        private static Sprite? _mist;
        private static Sprite? _fog;
        private static Sprite? _thread;

        public static Sprite Flame => _flame ??= CreateFlame();
        public static Sprite Glow => _glow ??= CreateGlow();
        public static Sprite Mist => _mist ??= CreateMist();
        public static Sprite Fog => _fog ??= CreateFog();
        public static Sprite Thread => _thread ??= Create(1, 1, new[] { new Color32(255, 255, 255, 255) }, new Vector2(0.5f, 1f));

        private static Sprite CreateFlame() {
            const int width = 64, height = 128;
            var pixels = new Color32[width * height];
            var core = new Color(1f, 0.96f, 0.78f);
            var edge = new Color(1f, 0.5f, 0.1f);
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var u = (x + 0.5f) / width * 2f - 1f;
                    var v = (y + 0.5f) / height;
                    var halfWidth = v < 0.25f
                        ? 0.8f * Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow((0.25f - v) / 0.25f, 2f)))
                        : 0.8f * Mathf.Pow(1f - (v - 0.25f) / 0.75f, 1.4f);
                    if (halfWidth <= 0.001f) continue;
                    var d = Mathf.Abs(u) / halfWidth;
                    if (d >= 1f) continue;
                    var fade = Mathf.Clamp01((d - 0.55f) / 0.45f);
                    var a = 1f - fade * fade * (3f - 2f * fade);
                    var heat = Mathf.Clamp01(1f - d) * Mathf.Clamp01(1.2f - v);
                    var color = Color.Lerp(edge, core, heat);
                    pixels[y * width + x] = new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(a * 255));
                }
            }
            return Create(width, height, pixels, new Vector2(0.5f, 0.12f));
        }

        private static Sprite CreateGlow() {
            const int size = 128;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                    var dx = (x + 0.5f) / size * 2f - 1f;
                    var dy = (y + 0.5f) / size * 2f - 1f;
                    var a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * a * a * 255));
                }
            }
            return Create(size, size, pixels, new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateMist() {
            const int size = 256;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                    var u = (float)x / size;
                    var v = (float)y / size;
                    var noise = 0.6f * Mathf.PerlinNoise(u * 4f, v * 4f) + 0.4f * Mathf.PerlinNoise(u * 9f + 13f, v * 9f + 7f);
                    var dx = u * 2f - 1f;
                    var dy = v * 2f - 1f;
                    var edge = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    var a = Mathf.Clamp01((noise - 0.35f) * 2.2f) * Mathf.SmoothStep(0f, 1f, edge * 1.6f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            return Create(size, size, pixels, new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateFog() {
            const int width = 256, height = 64;
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var v = (float)y / height;
                    var wisps = 0.65f + 0.35f * Mathf.PerlinNoise(x * 0.035f, y * 0.09f);
                    var a = Mathf.Clamp01(1f - v) * Mathf.Clamp01(1f - v) * wisps;
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            return Create(width, height, pixels, new Vector2(0.5f, 0f));
        }

        private static Sprite Create(int width, int height, Color32[] pixels, Vector2 pivot) {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot);
        }

        #endregion
    }
}
