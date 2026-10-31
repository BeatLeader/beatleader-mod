using System;
using System.Collections.Generic;
using System.Linq;
using HMUI;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using Screen = HMUI.Screen;

namespace BeatLeader.UI.MainMenu {
    internal class DanceEventMenuDecor : MonoBehaviour {
        #region Setup

        private static readonly Vector3 defaultHead = new(0f, 1.7f, 0f);

        private GameObject? _root;
        private readonly List<GameObject> _screenDecor = new();
        private MainMenuViewController? _mainMenu;

        private bool MainMenuShown => _mainMenu != null && _mainMenu.isActivated;

        private void Start() {
            DanceEvent.CurrentChangedEvent += Refresh;
            DanceEvent.Load();
            Refresh();
        }

        private void OnDestroy() {
            DanceEvent.CurrentChangedEvent -= Refresh;
            Despawn();
        }

        private void Refresh() {
            if (DanceEvent.Current != null) {
                Spawn();
            } else {
                Despawn();
            }
        }

        private void Spawn() {
            if (_root != null) return;
            _root = new GameObject("DanceEventMenuDecor");
            _root.transform.SetParent(transform, false);
            var random = new Random();
            var taken = new List<(Vector2 spot, float radius)>();
            _mainMenu = Resources.FindObjectsOfTypeAll<MainMenuViewController>().FirstOrDefault(x => x.gameObject.scene.isLoaded);
            SpawnCandles(random, taken);
            SpawnPumpkins(random, taken);
            SpawnMist(random);
            SpawnWebs(random, FindMainMenuScreenSystem());
            SpawnBats(random);
            SpawnSkeleton();
        }

        private ScreenSystem? FindMainMenuScreenSystem() {
            return _mainMenu?.screen?.GetComponentInParent<ScreenSystem>()
                ?? Resources.FindObjectsOfTypeAll<ScreenSystem>().FirstOrDefault(x => x.gameObject.scene.isLoaded && x.name == "ScreenSystem");
        }

        private void Despawn() {
            if (_root != null) Destroy(_root);
            _root = null;
            foreach (var decor in _screenDecor) {
                if (decor != null) Destroy(decor);
            }
            _screenDecor.Clear();
            _pumpkins.Clear();
            _candles.Clear();
            _candleLights.Clear();
            _mist.Clear();
            _webs.Clear();
            _thread = null;
            _bats.Clear();
            _skeleton = null;
        }

        private void Update() {
            if (_root == null) return;
            var camera = Camera.main;
            var head = camera != null ? camera.transform.position : defaultHead;
            var time = Time.time;
            UpdatePumpkins(head, time);
            UpdateCandles(head, time);
            UpdateMist();
            UpdateWebs(time);
            UpdateBats(head, time);
            UpdateSkeleton(head);
        }

        #endregion

        #region Utils

        private static bool TryPlace(Random random, List<(Vector2 spot, float radius)> taken, float fromDegrees, float toDegrees,
            float near, float far, float radius, out Vector2 spot) {
            for (var attempt = 0; attempt < 40; attempt++) {
                var angle = Mathf.Deg2Rad * Range(random, fromDegrees, toDegrees);
                var distance = Range(random, near, far);
                var candidate = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * distance;
                if (taken.Any(x => (x.spot - candidate).magnitude < x.radius + radius)) continue;
                taken.Add((candidate, radius));
                spot = candidate;
                return true;
            }
            spot = default;
            return false;
        }

        private static float Range(Random random, float from, float to) {
            return from + (float)random.NextDouble() * (to - from);
        }

        private static Vector3 OnFloor(Vector2 spot, float height = 0f) {
            return new Vector3(spot.x, height, spot.y);
        }

        private static void FaceHead(Transform transform, Vector3 head, float centerHeight, float maxLean) {
            var away = transform.position + Vector3.up * centerHeight - head;
            var flat = new Vector3(away.x, 0f, away.z);
            if (flat.sqrMagnitude < 0.0001f) return;
            var lean = Mathf.Min(maxLean, Vector3.Angle(flat, away));
            transform.rotation = Quaternion.LookRotation(flat) * Quaternion.Euler(lean, 0f, 0f);
        }

        private static Image AddImage(Transform parent, string name, Sprite? sprite, Material? material) {
            var image = material != null ? new GameObject(name).AddComponent<ImageView>() : new GameObject(name).AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.sprite = sprite;
            image.raycastTarget = false;
            if (material != null) image.material = material;
            return image;
        }

        private static (DanceEventModels.Model model, Material[] materials)? LoadModel(string file, Func<string, Material?> materialFor) {
            var model = DanceEventModels.Get(file);
            if (model == null) return null;
            var materials = model.Materials.Select(materialFor).OfType<Material>().ToArray();
            return materials.Length == model.Materials.Length ? (model, materials) : null;
        }

        private MeshRenderer CreateModel(string name, DanceEventModels.Model model, Material[] materials) {
            var go = new GameObject(name);
            go.transform.SetParent(_root!.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = model.Mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            return renderer;
        }

        private static float CandleFlicker(float time) {
            var breath = Mathf.PerlinNoise(time * 0.7f, 0.3f);
            var flutter = Mathf.PerlinNoise(time * 9f, 5.1f);
            return Mathf.Clamp01(0.45f + 0.35f * breath + 0.25f * flutter);
        }

        #endregion

        #region Pumpkins

        private const string PumpkinModelFile = "pumpkin_cracked.obj.gz";
        private const float PumpkinModelWidth = 0.5f;

        private static readonly int colorId = Shader.PropertyToID("_Color");

        private static readonly Dictionary<string, Color> pumpkinGlows = new() {
            { "glow", DanceEventModels.Hex("FFB347") },
            { "carved", DanceEventModels.Hex("C84A00") }
        };

        private readonly List<(MeshRenderer renderer, float phase, float yaw)> _pumpkins = new();
        private (int index, Color color)[] _pumpkinGlowParts = Array.Empty<(int, Color)>();
        private MaterialPropertyBlock? _pumpkinBlock;

        private static Material? PumpkinMaterial(string name) {
            if (pumpkinGlows.TryGetValue(name, out var glow)) return DanceEventModels.Glowing(glow);
            return name == "stem"
                ? DanceEventModels.Solid(DanceEventModels.Hex("4F4A22"), 0.15f)
                : DanceEventModels.Solid(DanceEventModels.Hex("CF6817"), 0.3f);
        }

        private void SpawnPumpkins(Random random, List<(Vector2 spot, float radius)> taken) {
            if (LoadModel(PumpkinModelFile, PumpkinMaterial) is not { } pumpkin) return;
            var (model, materials) = pumpkin;
            _pumpkinGlowParts = pumpkinGlows.Select(x => (model.IndexOf(x.Key), x.Value)).Where(x => x.Item1 >= 0).ToArray();
            var count = random.Next(5, 8);
            for (var attempt = 0; attempt < 20 && _pumpkins.Count < count; attempt++) {
                var front = random.NextDouble() < 0.7;
                var flank = random.Next(2) == 0 ? -1f : 1f;
                var placed = front
                    ? TryPlace(random, taken, -75f, 75f, 6f, 10f, 0.6f, out var spot)
                    : TryPlace(random, taken, flank * 80f, flank * 180f, 6f, 8.5f, 0.6f, out spot);
                if (!placed) continue;
                var size = Range(random, 0.4f, 0.7f) * (0.8f + 0.1f * spot.magnitude);
                AddPumpkin(spot, size);
                if (size < 0.55f || random.NextDouble() > 0.45) continue;
                var small = Range(random, 0.26f, 0.36f);
                var side = (float)(random.NextDouble() * Mathf.PI * 2);
                var next = spot + new Vector2(Mathf.Cos(side), Mathf.Sin(side)) * (size * 0.5f + small * 0.55f);
                if (taken.Any(x => x.spot != spot && (x.spot - next).magnitude < x.radius + small * 0.5f)) continue;
                taken.Add((next, small * 0.5f));
                AddPumpkin(next, small);
            }

            void AddPumpkin(Vector2 spot, float size) {
                var renderer = CreateModel("Pumpkin", model, materials);
                renderer.transform.position = OnFloor(spot);
                renderer.transform.localScale = Vector3.one * (size / PumpkinModelWidth);
                _pumpkins.Add((renderer, Range(random, 0f, 100f), Range(random, -30f, 30f)));
            }
        }

        private void UpdatePumpkins(Vector3 head, float time) {
            _pumpkinBlock ??= new MaterialPropertyBlock();
            foreach (var (renderer, phase, yaw) in _pumpkins) {
                var root = renderer.transform;
                var away = root.position - head;
                away.y = 0f;
                if (away.sqrMagnitude > 0.01f) root.rotation = Quaternion.LookRotation(-away) * Quaternion.Euler(0f, yaw, 0f);
                var flicker = CandleFlicker(time + phase);
                foreach (var (index, color) in _pumpkinGlowParts) {
                    _pumpkinBlock.SetColor(colorId, color * (0.45f + 0.75f * flicker));
                    renderer.SetPropertyBlock(_pumpkinBlock, index);
                }
            }
        }

        #endregion

        #region Candles

        private class CandleFlame {
            public Transform Root = null!;
            public Image Flame = null!;
            public float Phase;
            public float Center;
        }

        private const float WorldImageScale = 0.01f;

        private static readonly Color candleLightColor = new(1f, 0.55f, 0.2f);
        private readonly List<CandleFlame> _candles = new();
        private readonly List<(Image image, float phase)> _candleLights = new();

        private void SpawnCandles(Random random, List<(Vector2 spot, float radius)> taken) {
            var clusters = random.Next(3, 5);
            for (var c = 0; c < clusters; c++) {
                if (!TryPlace(random, taken, -70f, 70f, 3f, 3.9f, 0.4f, out var center)) break;
                var placed = new List<Vector2>();
                var count = random.Next(2, 5);
                for (var i = 0; i < count; i++) {
                    for (var attempt = 0; attempt < 20; attempt++) {
                        var offset = new Vector2(Range(random, -0.2f, 0.2f), Range(random, -0.14f, 0.14f));
                        if (placed.Any(x => (x - offset).magnitude < 0.12f)) continue;
                        placed.Add(offset);
                        var variant = DanceEventDecorSprites.Candles[random.Next(DanceEventDecorSprites.Candles.Length)];
                        AddCandle(center + offset, variant, Range(random, 0.3f, 0.42f), random);
                        break;
                    }
                }
                var light = DanceDreadApparitions.CreateImage("CandleLight", DanceEventDecorSprites.Glow, Vector2.one * Range(random, 1.1f, 1.5f), null, out var lightRoot);
                lightRoot.transform.SetParent(_root!.transform, false);
                lightRoot.transform.position = OnFloor(center, 0.01f);
                lightRoot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                _candleLights.Add((light, Range(random, 0f, 100f)));
            }
        }

        private void AddCandle(Vector2 spot, DanceEventDecorSprites.Candle variant, float scale, Random random) {
            if (variant.Sprite is not { } sprite) return;
            var body = DanceDreadApparitions.CreateImage("Candle", sprite, DanceEventDecorSprites.CandleSpriteSize * scale, GameResources.UINoGlowMaterial, out var root);
            ((RectTransform)root.transform).pivot = new Vector2(0.5f, variant.Bottom);
            root.transform.SetParent(_root!.transform, false);
            root.transform.position = OnFloor(spot);
            var flame = AddImage(body.transform, "Flame", DanceEventDecorSprites.Flame, null);
            var rect = flame.rectTransform;
            rect.anchorMin = rect.anchorMax = variant.Wick;
            rect.pivot = new Vector2(0.5f, 0.12f);
            rect.sizeDelta = new Vector2(0.12f, 0.26f) * scale / WorldImageScale;
            _candles.Add(new CandleFlame { Root = root.transform, Flame = flame, Phase = Range(random, 0f, 100f), Center = scale * 0.4f });
        }

        private void UpdateCandles(Vector3 head, float time) {
            foreach (var candle in _candles) {
                FaceHead(candle.Root, head, candle.Center, 20f);
                var t = time + candle.Phase;
                var stretch = Mathf.PerlinNoise(t * 7f, 1.3f);
                var sway = Mathf.PerlinNoise(t * 1.7f, 8.2f) - 0.5f;
                var flame = candle.Flame.rectTransform;
                flame.localScale = new Vector3(1.05f - 0.12f * stretch, 0.8f + 0.4f * stretch, 1f);
                flame.localRotation = Quaternion.Euler(0f, 0f, sway * 10f);
                candle.Flame.color = new Color(1f, 1f, 1f, 0.8f + 0.2f * Mathf.PerlinNoise(t * 11f, 4.4f));
            }
            foreach (var (image, phase) in _candleLights) {
                image.color = candleLightColor.ColorWithAlpha(0.22f * (0.6f + 0.4f * CandleFlicker(time + phase)));
            }
        }

        #endregion

        #region Mist

        private static readonly (float height, float size, float degreesPerSecond)[] mistLayers = {
            (0.06f, 9f, 2.5f),
            (0.18f, 7f, -3.5f),
            (0.32f, 11f, 1.5f)
        };

        private static readonly Color mistColor = new(0.62f, 0.55f, 0.78f, 0.07f);
        private readonly List<(Transform root, float degreesPerSecond)> _mist = new();

        private void SpawnMist(Random random) {
            foreach (var (height, size, degreesPerSecond) in mistLayers) {
                var image = DanceDreadApparitions.CreateImage("Mist", DanceEventDecorSprites.Mist, Vector2.one * size, null, out var root);
                root.transform.SetParent(_root!.transform, false);
                root.transform.position = new Vector3(0f, height, 0f);
                root.transform.rotation = Quaternion.Euler(90f, Range(random, 0f, 360f), 0f);
                image.color = mistColor;
                _mist.Add((root.transform, degreesPerSecond));
            }
        }

        private void UpdateMist() {
            foreach (var (root, degreesPerSecond) in _mist) {
                root.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
            }
        }

        #endregion

        #region Webs

        private class Web {
            public Screen Screen = null!;
            public Canvas Canvas = null!;
            public RectTransform Rect = null!;
            public bool RightCorner;
        }

        private const float WebLayoutInterval = 0.5f;
        private const float WebOutsideCorner = 0.5f;

        private static readonly Color webColor = new(0.86f, 0.86f, 0.92f, 0.6f);
        private static readonly List<Image> screenImages = new();
        private static readonly Vector3[] imageCorners = new Vector3[4];

        private readonly List<Web> _webs = new();
        private float _nextWebLayout;
        private bool _websShown;
        private RectTransform? _thread;
        private float _threadPhase;

        private void SpawnWebs(Random random, ScreenSystem? system) {
            if (system == null || DanceEventDecorSprites.Web == null) return;
            var left = AddWeb(system.leftScreen, false, Range(random, 20f, 26f));
            var right = AddWeb(system.rightScreen, true, Range(random, 20f, 26f));
            AddWeb(system.mainScreen, random.Next(2) == 0, Range(random, 14f, 18f));
            var spiderWeb = random.Next(2) == 0 ? left : right;
            if (spiderWeb != null) AddSpider(spiderWeb);
            _threadPhase = Range(random, 0f, 100f);
        }

        private Image? AddWeb(Screen? screen, bool rightCorner, float size) {
            if (screen == null) return null;
            var parentCanvas = screen.GetComponent<Canvas>();
            var go = new GameObject("DanceEventWeb");
            go.layer = screen.gameObject.layer;
            go.transform.SetParent(screen.transform, false);
            _screenDecor.Add(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 0) + 10;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(rightCorner ? 1f : 0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = Vector2.one * size;
            rect.localScale = new Vector3(rightCorner ? -1f : 1f, 1f, 1f);
            var image = AddImage(go.transform, "Web", DanceEventDecorSprites.Web, GameResources.UINoGlowMaterial);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;
            image.color = webColor;
            _webs.Add(new Web { Screen = screen, Canvas = canvas, Rect = rect, RightCorner = rightCorner });
            return image;
        }

        private void AddSpider(Image web) {
            if (DanceEventDecorSprites.Spider == null) return;
            var thread = AddImage(web.transform, "Thread", DanceEventDecorSprites.Thread, GameResources.UINoGlowMaterial);
            thread.color = webColor.ColorWithAlpha(0.5f);
            _thread = thread.rectTransform;
            _thread.anchorMin = _thread.anchorMax = new Vector2(0.4f, 0.6f);
            _thread.pivot = new Vector2(0.5f, 1f);
            _thread.sizeDelta = new Vector2(0.25f, 15f);
            var spider = AddImage(_thread, "Spider", DanceEventDecorSprites.Spider, GameResources.UINoGlowMaterial);
            var rect = spider.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.8f);
            rect.sizeDelta = Vector2.one * 12f;
        }

        private void LayoutWebs() {
            foreach (var web in _webs) {
                if (web.Canvas == null) continue;
                if (web.Screen == null || !TryGetVisibleBounds(web.Screen, out var bounds)) {
                    web.Canvas.enabled = false;
                    continue;
                }
                web.Canvas.enabled = true;
                var corner = new Vector2(web.RightCorner ? bounds.xMax : bounds.xMin, bounds.yMax);
                web.Rect.localPosition = corner + new Vector2(web.RightCorner ? WebOutsideCorner : -WebOutsideCorner, WebOutsideCorner);
            }
        }

        private bool TryGetVisibleBounds(Screen screen, out Rect bounds) {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            screen.GetComponentsInChildren(false, screenImages);
            foreach (var image in screenImages) {
                if (!image.enabled || image.color.a < 0.05f || image.canvasRenderer.GetInheritedAlpha() < 0.05f) continue;
                if (_webs.Any(x => image.transform.IsChildOf(x.Rect))) continue;
                image.rectTransform.GetWorldCorners(imageCorners);
                foreach (var corner in imageCorners) {
                    var local = (Vector2)screen.transform.InverseTransformPoint(corner);
                    min = Vector2.Min(min, local);
                    max = Vector2.Max(max, local);
                }
            }
            bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return max.x > min.x;
        }

        private void UpdateWebs(float time) {
            if (!MainMenuShown) {
                foreach (var web in _webs) {
                    if (web.Canvas != null) web.Canvas.enabled = false;
                }
                _websShown = false;
                return;
            }
            if (!_websShown || time >= _nextWebLayout) {
                LayoutWebs();
                _nextWebLayout = time + WebLayoutInterval;
                _websShown = true;
            }
            if (_thread == null) return;
            var t = time + _threadPhase;
            var length = 10f + 14f * (0.5f + 0.5f * Mathf.Sin(t * 0.3f));
            _thread.sizeDelta = new Vector2(0.25f, length);
            _thread.localRotation = Quaternion.Euler(0f, 0f, 3f * Mathf.Sin(t * 0.9f));
        }

        #endregion

        #region Skeleton

        private const string SkeletonModelFile = "skeleton_leaner.obj.gz";

        private static readonly Vector3 skeletonBackContact = new(0.422f, 1.153f, -0.313f);

        private static readonly Dictionary<string, (string color, float minimumLight)> skeletonMaterials = new() {
            { "bone", ("EFE6D2", 0.35f) },
            { "eye_sockets", ("140C0A", 0f) },
            { "hat", ("151515", 0.1f) },
            { "hat_band", ("FF6A00", 0.3f) }
        };

        private static readonly Vector3[] panelCorners = new Vector3[4];

        private GameObject? _skeleton;

        private static Material? SkeletonMaterial(string name) {
            return skeletonMaterials.TryGetValue(name, out var look)
                ? DanceEventModels.Solid(DanceEventModels.Hex(look.color), look.minimumLight)
                : DanceEventModels.Glowing(DanceEventModels.Hex("FF8A1A"));
        }

        private void SpawnSkeleton() {
            if (LoadModel(SkeletonModelFile, SkeletonMaterial) is not { } skeleton) return;
            _skeleton = CreateModel("DanceEventSkeleton", skeleton.model, skeleton.materials).gameObject;
            _skeleton.SetActive(false);
        }

        private void UpdateSkeleton(Vector3 head) {
            if (_skeleton == null) return;
            var panel = DanceEventPanel.Shown;
            var show = panel != null && panel.gameObject.activeInHierarchy && MainMenuShown;
            if (_skeleton.activeSelf != show) _skeleton.SetActive(show);
            if (!show) return;
            panel!.GetWorldCorners(panelCorners);
            var bottom = OnCurvedScreen(panel, panelCorners[3]);
            var top = OnCurvedScreen(panel, panelCorners[2]);
            var edge = Vector3.LerpUnclamped(bottom, top, Mathf.InverseLerp(bottom.y, top.y, skeletonBackContact.y));
            var toHead = head - edge;
            toHead.y = 0f;
            if (toHead.sqrMagnitude < 0.01f) return;
            var rotation = Quaternion.LookRotation(toHead);
            var position = edge - rotation * skeletonBackContact;
            position.y = 0f;
            _skeleton.transform.SetPositionAndRotation(position, rotation);
        }

        private static Vector3 OnCurvedScreen(RectTransform content, Vector3 flat) {
            var curved = content.GetComponentInParent<CurvedCanvasSettings>();
            if (curved == null || curved.radius <= 0f) return flat;
            var canvas = curved.transform;
            var local = canvas.InverseTransformPoint(flat);
            var angle = local.x / curved.radius;
            var bent = new Vector3(curved.radius * Mathf.Sin(angle), local.y, local.z - curved.radius * (1f - Mathf.Cos(angle)));
            return canvas.TransformPoint(bent);
        }

        #endregion

        #region Bats

        private class Bat {
            public Transform Root = null!;
            public Image Image = null!;
            public Vector3 Center;
            public float Radius;
            public float Speed;
            public float Angle;
            public float Flap;
            public float Phase;
        }

        private static readonly Vector3 defaultLogoPosition = new(0f, 19f, 26f);
        private readonly List<Bat> _bats = new();

        private void SpawnBats(Random random) {
            if (DanceEventDecorSprites.BatUp is not { } batUp || DanceEventDecorSprites.BatDown == null) return;
            var logo = GameObject.Find("Logo")?.transform.position ?? defaultLogoPosition;
            var count = random.Next(3, 6);
            for (var i = 0; i < count; i++) {
                var width = Range(random, 1.5f, 2.4f);
                var image = DanceDreadApparitions.CreateImage("Bat", batUp, new Vector2(width, width * 0.625f), GameResources.UINoGlowMaterial, out var root);
                root.transform.SetParent(_root!.transform, false);
                _bats.Add(new Bat {
                    Root = root.transform,
                    Image = image,
                    Center = logo + new Vector3(Range(random, -3f, 3f), Range(random, -4f, 3f), 0f),
                    Radius = Range(random, 6f, 11f),
                    Speed = Range(random, 12f, 24f) * (random.Next(2) == 0 ? -1f : 1f),
                    Angle = Range(random, 0f, 360f),
                    Flap = Range(random, 4f, 6f),
                    Phase = Range(random, 0f, 10f)
                });
            }
        }

        private void UpdateBats(Vector3 head, float time) {
            foreach (var bat in _bats) {
                bat.Angle += bat.Speed * Time.deltaTime;
                var angle = bat.Angle * Mathf.Deg2Rad;
                var t = time + bat.Phase;
                var wingBeat = Mathf.Sin(t * bat.Flap * Mathf.PI * 2f);
                var offset = new Vector3(Mathf.Cos(angle) * bat.Radius, 0.4f * wingBeat + 1.2f * Mathf.Sin(t * 0.4f), Mathf.Sin(angle) * bat.Radius * 0.35f);
                bat.Root.position = bat.Center + offset;
                var away = bat.Root.position - head;
                if (away.sqrMagnitude > 0.01f) bat.Root.rotation = Quaternion.LookRotation(away, Vector3.up);
                bat.Image.sprite = wingBeat > 0f ? DanceEventDecorSprites.BatUp : DanceEventDecorSprites.BatDown;
            }
        }

        #endregion
    }
}
