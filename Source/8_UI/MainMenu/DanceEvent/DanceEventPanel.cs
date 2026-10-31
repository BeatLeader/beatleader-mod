using System;
using System.Globalization;
using System.Linq;
using System.Text;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.MainMenu {
    internal class DanceEventPanel : ReactiveComponent {
        #region Public API

        public Action? OnBackClick { get; set; }

        public static RectTransform? Shown { get; set; }

        private PlatformEvent? _event;

        public void SetEvent(PlatformEvent evt) {
            _event = evt;
            _header.Text = evt.name;
            _playlistButton.Interactable = evt.downloadable;
        }

        public void SetLoading() {
            SetMessage("Summoning the standings...");
        }

        public void SetFailed() {
            SetMessage("The spirits are silent, try again later");
        }

        public void SetData(DanceEventStatus status) {
            var inv = CultureInfo.InvariantCulture;
            var counted = status.rules?.countedMaps ?? 5;
            var player = status.player;
            _header.Text = status.eventDescription.name;
            _points.Text = $"{player?.points.ToString("F1", inv) ?? "0"}<size=55%> / {counted * 100} points</size>";
            if (player == null) {
                _rank.Text = $"{status.playersCount} dancers so far, play an event map with Dance Mode to join";
                _maps.Text = "";
                return;
            }
            _rank.Text = $"#{player.rank} of {status.playersCount}  <color=#AAAAAA>#{player.countryRank} in your country</color>\n" +
                $"<color=#C9A0FF>{player.dancingMaps} {(player.dancingMaps == 1 ? "map" : "maps")} above the dancing line</color>";
            var lines = new StringBuilder();
            lines.Append($"<color=#AAAAAA>Your best {counted}</color>\n");
            foreach (var map in player.maps.Where(x => x.counted).Take(counted)) {
                var mark = map.dancing ? "<color=#FF9A1F>•</color>" : "<color=#555555>•</color>";
                lines.Append($"{mark} {MapName(map)}  <color=#FFD700>{map.points.ToString("F1", inv)}</color>\n");
            }
            _maps.Text = lines.ToString();
        }

        private void SetMessage(string message) {
            _points.Text = "";
            _rank.Text = message;
            _maps.Text = "";
        }

        private static string MapName(DanceEventMap map) {
            var level = SongCore.Loader.GetLevelByHash(map.hash);
            if (level != null) {
                var name = level.songName;
                return name.Length > 28 ? name.Substring(0, 27) + "…" : name;
            }
            var id = map.leaderboardId;
            var beatSaverKey = id.Length > 2 ? id.Substring(0, id.Length - 2) : id;
            return $"map {beatSaverKey}";
        }

        #endregion

        #region Construct

        private Label _header = null!;
        private Label _points = null!;
        private Label _rank = null!;
        private Label _maps = null!;
        private BsButton _playlistButton = null!;

        private void OpenPlaylistDialog() {
            if (_event == null) return;
#pragma warning disable CS0618 // Type or member is obsolete
            ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, _event);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static readonly Color pumpkinOrange = new(1f, 0.48f, 0.1f);

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new Image {
                            Sprite = DanceEventDecorSprites.Fog,
                            Color = new Color(0.45f, 0.25f, 0.6f, 0.55f),
                            RaycastTarget = false
                        }.AsFlexItem(position: new() { left = 0.pt(), right = 0.pt(), bottom = 0.pt() }, size: new() { y = 22.pt() }),
                        new Background {
                            Sprite = BundleLoader.Sprites.background,
                            PixelsPerUnit = 10f,
                            Children = {
                                new Label {
                                    Text = "Dance or Die!",
                                    RichText = true,
                                    FontSize = 3.5f,
                                    FontStyle = FontStyles.Italic,
                                    Alignment = TextAlignmentOptions.Capline
                                }.AsFlexItem().Bind(ref _header)
                            }
                        }.AsFlexGroup(justifyContent: Justify.Center, alignItems: Align.Center)
                        .AsBackground(color: pumpkinOrange.ColorWithAlpha(0.8f))
                        .AsFlexItem(size: new() { y = 5 }, margin: new() { bottom = 1 }),
                        new Layout {
                            Children = {
                                new Image {
                                    Sprite = DanceEventDecorSprites.Pumpkin,
                                    PreserveAspect = true,
                                    RaycastTarget = false
                                }.AsFlexItem(size: new() { x = 13, y = 13 }),
                                new Layout {
                                    Children = {
                                        new Label {
                                            RichText = true,
                                            FontSize = 6f,
                                            Alignment = TextAlignmentOptions.Left,
                                            Color = pumpkinOrange
                                        }.AsFlexItem(size: new() { y = 7 }).Bind(ref _points),
                                        new Label {
                                            RichText = true,
                                            FontSize = 2.6f,
                                            Alignment = TextAlignmentOptions.TopLeft,
                                            EnableWrapping = true
                                        }.AsFlexItem(size: new() { y = 7 }).Bind(ref _rank)
                                    }
                                }.AsFlexGroup(direction: FlexDirection.Column).AsFlexItem(flex: 1, margin: new() { left = 1.5f })
                            }
                        }.AsFlexGroup(direction: FlexDirection.Row, alignItems: Align.Center).AsFlexItem(size: new() { y = 14 }),
                        new Label {
                            RichText = true,
                            FontSize = 2.6f,
                            Alignment = TextAlignmentOptions.TopLeft,
                            EnableWrapping = false,
                            Overflow = TextOverflowModes.Ellipsis
                        }.AsFlexItem(flex: 1, margin: new() { left = 2, top = 1 }).Bind(ref _maps),
                        new Layout {
                            Children = {
                                new BsButton {
                                    Text = "←",
                                    ShowUnderline = false,
                                    Skew = 0f,
                                    OnClick = () => OnBackClick?.Invoke()
                                }.AsFlexItem(size: new() { x = 8, y = 5 }),
                                new BsButton {
                                    Text = "💿 Get the maps",
                                    ShowUnderline = false,
                                    Skew = 0f,
                                    Interactable = false,
                                    OnClick = OpenPlaylistDialog
                                }.AsFlexItem(size: new() { y = 5 }).Bind(ref _playlistButton)
                            }
                        }.AsFlexGroup(direction: FlexDirection.Row, justifyContent: Justify.SpaceBetween).AsFlexItem(size: new() { y = 5 })
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.6f))
                .AsRectMask()
                .AsFlexGroup(direction: FlexDirection.Column, padding: 1.pt())
                .AsFlexItem(flex: 1f, size: new() { x = 52.pt() })
                .Use();
        }

        #endregion
    }
}
