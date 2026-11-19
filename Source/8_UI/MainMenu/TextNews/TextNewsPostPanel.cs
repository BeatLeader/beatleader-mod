using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostPanel : ReactiveComponent {
        public TextNewsPostPanel(CellContext<NewsPost> context) : base(false) {
            _context = context;
            ConstructAndInit();
        }

        private readonly CellContext<NewsPost> _context;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    Gap = 1,
                    FlexDirection = FlexDirection.Column
                },

                Children = {
                    new TextNewsPostHeaderPanel {
                        sOwner = _context.Map(x => x.Item.owner),
                        sTimestamp = _context.Map(x => x.Item.timepost),
                        sAvatarUrl = _context.Map(x => x.Item.ownerIcon)
                    },

                    new Label {
                        FlexItem = {
                            Margin = new() { left = 1.pt, right = 1.pt }
                        },

                        FontSize = 3,
                        EnableWrapping = true,
                        Alignment = TMPro.TextAlignmentOptions.Justified,
                        sText = _context.Map(x => x.Item.body)
                    },

                    new WebImage {
                        FlexItem = {
                            Size = new() { y = 30 }
                        },

                        PreserveAspect = true,
                        Material = GameResources.UINoGlowMaterial,
                        sEnabled = _context.Map(x => !string.IsNullOrEmpty(x.Item.image)),
                        sSrc = _context.Map(x => x.Item.image)
                    }
                }
            }.Use();
        }
    }
}