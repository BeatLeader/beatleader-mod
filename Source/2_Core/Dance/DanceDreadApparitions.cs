using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    internal class DanceDreadApparitions {
        public void Preload() {
            _ = AuraSprite;
        }

        public void Update(float silhouetteStage, float eyesStage, int stops, float nearDeath, float dt) {
            var camera = Camera.main;
            if (camera == null) return;
            var head = camera.transform.position;
            UpdateEyes(eyesStage, head, dt);
            UpdateSilhouette(silhouetteStage, stops, nearDeath, head, dt);
        }

        public void Dispose() {
            foreach (var pair in _eyes) {
                if (pair.Root != null) Object.Destroy(pair.Root);
            }
            _eyes.Clear();
            if (_silhouette != null) Object.Destroy(_silhouette);
        }

        #region Eyes

        private static readonly Color eyeColor = new(1f, 0.28f, 0.06f);

        private static readonly (float angle, float distance, float height, float threshold)[] eyePlaces = {
            (-62f, 26f, 1.4f, 0f),
            (48f, 31f, 2.6f, 0.15f),
            (-34f, 34f, 3.4f, 0.3f),
            (74f, 22f, 0.9f, 0.45f),
            (-80f, 20f, 2.1f, 0.6f)
        };

        private class EyePair {
            public GameObject Root = null!;
            public Image Image = null!;
            public float Threshold;
            public float BlinkTimer;
            public float NextBlink;
        }

        private readonly List<EyePair> _eyes = new();

        private void UpdateEyes(float stage, Vector3 head, float dt) {
            if (_eyes.Count == 0) {
                if (stage <= 0f) return;
                foreach (var (angle, distance, height, threshold) in eyePlaces) {
                    var image = CreateImage("DanceDreadEyes", EyesSprite, new Vector2(1.2f, 0.45f), null, out var root);
                    root.transform.position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance + Vector3.up * height;
                    _eyes.Add(new EyePair { Root = root, Image = image, Threshold = threshold, NextBlink = Random.Range(2f, 6f) });
                }
            }
            foreach (var pair in _eyes) {
                Face(pair.Root.transform, head);
                pair.Image.color = WithAlpha(eyeColor, Mathf.Clamp01((stage - pair.Threshold) / 0.25f));
                pair.BlinkTimer += dt;
                var closed = pair.BlinkTimer >= pair.NextBlink;
                if (pair.BlinkTimer >= pair.NextBlink + 0.14f) {
                    pair.BlinkTimer = 0f;
                    pair.NextBlink = Random.Range(2.5f, 7f);
                }
                pair.Root.transform.localScale = new Vector3(1f, closed ? 0.1f : 1f, 1f) * ImageScale;
            }
        }

        private static Sprite? _eyesSprite;

        private static Sprite EyesSprite {
            get {
                if (_eyesSprite != null) return _eyesSprite;
                const int width = 192, height = 72;
                var centers = new[] { 0.27f, 0.73f };
                var alpha = new float[width * height];
                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        var u = (x + 0.5f) / width;
                        var v = (y + 0.5f) / height * 2f - 1f;
                        var a = 0f;
                        foreach (var center in centers) {
                            var dx = (u - center) / 0.19f;
                            var halfHeight = 0.8f * (1f - dx * dx);
                            if (halfHeight <= 0f) continue;
                            var dy = Mathf.Abs(v) / halfHeight;
                            var eye = Mathf.Clamp01((1f - dy) * 3f);
                            var pupil = Mathf.Clamp01(1f - Mathf.Abs(dx) / 0.09f) * 0.85f;
                            var glow = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx / 1.96f + v * v)) * 0.35f;
                            a = Mathf.Max(a, Mathf.Max(eye * (1f - pupil), glow));
                        }
                        alpha[y * width + x] = a;
                    }
                }
                _eyesSprite = CreateAlphaSprite(width, height, alpha, new Vector2(0.5f, 0.5f));
                return _eyesSprite;
            }
        }

        #endregion

        #region Silhouette

        private const float SilhouetteHeight = 2.3f;
        private const float SilhouetteEyesY = 0.943f;
        private const float StartSilhouetteDistance = 22f;
        private const float MinSilhouetteDistance = 4f;
        private const float DeathDistance = 1.5f;
        private const int AuraPadding = 40;

        private static readonly Color auraColor = new(0.75f, 0.12f, 0.3f);

        private GameObject? _silhouette;
        private Image? _silhouetteImage;
        private Image? _silhouetteEyes;
        private Image? _aura;
        private float _distance = StartSilhouetteDistance;
        private int _placedStops;

        private float _jumpTimer;
        private float _nextJump;
        private float _hidden;
        private Vector3 _nextSpot;
        private float _flare;
        private bool _closingIn;

        private float _dropout;
        private float _nextDropout = 1f;

        private void UpdateSilhouette(float stage, int stops, float nearDeath, Vector3 head, float dt) {
            if (_silhouette == null) {
                if (stage <= 0f || SilhouetteSprite == null) return;
                CreateSilhouette();
            }
            var stopped = stops != _placedStops;
            if (stopped) {
                _placedStops = stops;
                _distance = Mathf.Max(MinSilhouetteDistance, StartSilhouetteDistance * Mathf.Pow(0.8f, Mathf.Max(0, stops - 1)));
            }
            var closingIn = nearDeath > 0f;
            _jumpTimer += dt;
            if (stage > 0.05f && _hidden <= 0f && (stopped || _jumpTimer >= _nextJump || closingIn != _closingIn)) {
                _jumpTimer = 0f;
                _nextJump = closingIn ? Random.Range(0.5f, 0.9f) : Mathf.Lerp(7f, 2.2f, stage) * Random.Range(0.7f, 1.3f);
                _nextSpot = PickSpot(stage, nearDeath, head);
                _hidden = Random.Range(0.12f, 0.35f);
            }
            _closingIn = closingIn;
            var transform = _silhouette!.transform;
            if (_hidden > 0f) {
                _hidden -= dt;
                if (_hidden <= 0f) {
                    transform.position = _nextSpot;
                    _flare = 1f;
                }
            }
            Face(transform, head);

            _nextDropout -= dt;
            if (_dropout > 0f) {
                _dropout -= dt;
            } else if (_nextDropout <= 0f) {
                _dropout = Random.Range(0.04f, 0.12f);
                _nextDropout = Random.Range(0.3f, 2.5f) * Mathf.Lerp(1.5f, 0.4f, Mathf.Max(stage, nearDeath));
            }
            _flare = Mathf.Max(0f, _flare - dt * 2.5f);
            var visibility = _hidden > 0f ? 0f : _dropout > 0f ? 0.15f : 1f;
            var shimmer = 0.75f + 0.25f * Mathf.PerlinNoise(Time.time * 6f, 3.7f);
            var alpha = Mathf.Clamp01(Mathf.Max(stage * 1.2f, nearDeath)) * visibility;
            _silhouetteImage!.color = new Color(1f, 1f, 1f, alpha);
            _silhouetteEyes!.color = WithAlpha(eyeColor, Mathf.Clamp01(alpha * alpha + _flare * visibility));
            _aura!.color = WithAlpha(auraColor, Mathf.Clamp01((0.9f * shimmer + 0.6f * _flare) * alpha));
        }

        private Vector3 PickSpot(float stage, float nearDeath, Vector3 head) {
            var feet = new Vector3(head.x, 0f, head.z);
            if (nearDeath > 0f) {
                return feet + new Vector3(Random.Range(-0.35f, 0.35f), 0f, DeathDistance);
            }
            var distance = Mathf.Max(3f, _distance * Mathf.Lerp(1f, 0.55f, stage) * Random.Range(0.85f, 1.1f));
            var side = Random.value < 0.5f ? -1f : 1f;
            var angle = Random.value < 0.3f ? side * Random.Range(35f, 60f) : Random.Range(-15f, 15f);
            return feet + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        }

        private void CreateSilhouette() {
            var sprite = SilhouetteSprite!;
            var width = SilhouetteHeight * sprite.rect.width / sprite.rect.height;
            _silhouetteImage = CreateImage("DanceDreadSilhouette", sprite, new Vector2(width, SilhouetteHeight), GameResources.UINoGlowMaterial, out _silhouette);
            ((RectTransform)_silhouette.transform).pivot = new Vector2(0.5f, 0f);
            _silhouette.transform.position = Vector3.forward * _distance;

            _aura = AddImage("Aura", AuraSprite, _silhouette.transform, null);
            _aura.transform.SetAsFirstSibling();
            var padding = AuraPadding * (SilhouetteHeight / ImageScale) / sprite.rect.height;
            _aura.rectTransform.anchorMin = Vector2.zero;
            _aura.rectTransform.anchorMax = Vector2.one;
            _aura.rectTransform.offsetMin = new Vector2(-padding, -padding);
            _aura.rectTransform.offsetMax = new Vector2(padding, padding);

            _silhouetteEyes = AddImage("Eyes", EyesSprite, _silhouetteImage.transform, null);
            var eyes = _silhouetteEyes.rectTransform;
            eyes.anchorMin = eyes.anchorMax = new Vector2(0.5f, SilhouetteEyesY);
            eyes.sizeDelta = new Vector2(0.32f, 0.12f) / ImageScale;
        }

        private static Sprite? _auraSprite;

        private static Sprite? AuraSprite {
            get {
                if (_auraSprite != null || SilhouetteSprite == null) return _auraSprite;
                var source = SilhouetteSprite.texture;
                var pixels = source.GetPixels32();
                int w = source.width, h = source.height, p = AuraPadding;
                int width = w + 2 * p, height = h + 2 * p;
                var body = new float[width * height];
                for (var y = 0; y < h; y++) {
                    for (var x = 0; x < w; x++) body[(y + p) * width + x + p] = pixels[y * w + x].a / 255f;
                }
                var glow = (float[])body.Clone();
                for (var pass = 0; pass < 3; pass++) {
                    BoxBlur(glow, width, height, 12, true);
                    BoxBlur(glow, width, height, 12, false);
                }
                for (var i = 0; i < glow.Length; i++) glow[i] = Mathf.Clamp01(glow[i] * 2.2f) * (1f - body[i]);
                _auraSprite = CreateAlphaSprite(width, height, glow, new Vector2(0.5f, 0f));
                return _auraSprite;
            }
        }

        private static void BoxBlur(float[] data, int width, int height, int radius, bool horizontal) {
            var count = horizontal ? height : width;
            var length = horizontal ? width : height;
            var line = new float[length];
            for (var i = 0; i < count; i++) {
                for (var j = 0; j < length; j++) line[j] = data[horizontal ? i * width + j : j * width + i];
                var sum = 0f;
                for (var j = -radius; j <= radius; j++) sum += j >= 0 && j < length ? line[j] : 0f;
                for (var j = 0; j < length; j++) {
                    data[horizontal ? i * width + j : j * width + i] = sum / (2 * radius + 1);
                    var add = j + radius + 1;
                    var remove = j - radius;
                    if (add < length) sum += line[add];
                    if (remove >= 0) sum -= line[remove];
                }
            }
        }

        private static Sprite? _silhouetteSprite;
        private static bool _silhouetteLoaded;

        private static Sprite? SilhouetteSprite {
            get {
                if (_silhouetteLoaded) return _silhouetteSprite;
                _silhouetteLoaded = true;
                _silhouetteSprite = LoadSprite("silhouette.png", new Vector2(0.5f, 0f));
                return _silhouetteSprite;
            }
        }

        #endregion

        #region Utils

        private const float ImageScale = 0.01f;

        internal static Image CreateImage(string name, Sprite sprite, Vector2 size, Material? material, out GameObject root) {
            root = new GameObject(name);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            //ImageView writes curve data there, without it the game's UI shaders collapse the quad to a line
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
            ((RectTransform)root.transform).sizeDelta = size / ImageScale;
            root.transform.localScale = Vector3.one * ImageScale;
            var image = AddImage("Image", sprite, root.transform, material);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;
            return image;
        }

        private static Image AddImage(string name, Sprite? sprite, Transform parent, Material? material) {
            var gameObject = new GameObject(name);
            //the game's UI materials draw nothing on a plain Image
            var image = material != null ? gameObject.AddComponent<HMUI.ImageView>() : gameObject.AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.sprite = sprite;
            image.raycastTarget = false;
            if (material != null) image.material = material;
            return image;
        }

        internal static Sprite CreateAlphaSprite(int width, int height, float[] alpha, Vector2 pivot) {
            var pixels = new Color32[alpha.Length];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, (byte)(alpha[i] * 255));
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot);
        }

        internal static Sprite? LoadSprite(string file, Vector2 pivot) {
            using var stream = ResourcesUtils.GetEmbeddedResourceStream($"{Plugin.ResourcesPath}.Halloween.{file}");
            if (stream == null) return null;
            var bytes = new byte[stream.Length];
            _ = stream.Read(bytes, 0, bytes.Length);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            if (!texture.LoadImage(bytes)) return null;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
        }

        private static Color WithAlpha(Color color, float alpha) {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static void Face(Transform transform, Vector3 head) {
            var away = transform.position - head;
            away.y = 0f;
            if (away.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(away);
        }

        #endregion
    }
}
