using System.Linq;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeatLeader.UI.Hub {
    internal class TagEditorPanel : ReactiveComponent {
        #region Tag

        public string TagName => _nameContext.Text ?? string.Empty;
        public Color TagColor => palette[_colorIndex.Value];
        public IState<bool> Validity => _validity;

        public void Clear() {
            _colorIndex.Value = Random.Range(0, palette.Length);
            _nameContext.Text = null;
        }

        #endregion

        #region Construct

        private static readonly Color[] palette = Enumerable
            .Range(0, 8)
            .Select(static x => Color.HSVToRGB(x / 8f, 0.65f, 1f))
            .ToArray();

        private readonly BsInputFieldContext _nameContext = new() {
            Validate = static x => x is not { Length: > 10 }
        };

        private State<int> _colorIndex = null!;
        private IState<bool> _validity = null!;

        protected override GameObject Construct() {
            _colorIndex = Remember(Random.Range(0, palette.Length));
            _validity = _nameContext.Map(static x => x.Text != null && ReplayMetadataManager.ValidateTagName(x.Text).Ok);

            var inputAnchor = Ref<RectTransform>();

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 1.5f
                },

                Children = {
                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f
                        },

                        Context = _nameContext,
                        Placeholder = "Enter Name",
                        Icon = GameResources.Sprites.EditIcon
                    }.Bind(inputAnchor),

                    new BsKeyboardModal {
                        Context = _nameContext,
                        PlacementAnchor = inputAnchor
                    },

                    new Layout {
                        FlexController = {
                            AlignItems = Align.Center,
                            JustifyContent = Justify.SpaceBetween,
                            Padding = new() { left = 1f, right = 1f }
                        },

                        FlexItem = {
                            Size = new() { y = 4.5f }
                        },

                        Children = {
                            CreateSwatch(0, _colorIndex),
                            CreateSwatch(1, _colorIndex),
                            CreateSwatch(2, _colorIndex),
                            CreateSwatch(3, _colorIndex),
                            CreateSwatch(4, _colorIndex),
                            CreateSwatch(5, _colorIndex),
                            CreateSwatch(6, _colorIndex),
                            CreateSwatch(7, _colorIndex)
                        }
                    }
                }
            }.Use();
        }

        [StateGen]
        private static Image CreateSwatch(int index, State<int> selected) {
            var color = palette[index];

            return new Image {
                FlexItem = {
                    Size = 3.5f
                },

                ContentTransform = {
                    slocalScale = selected.Map(x => Vector3.one * (x == index ? 1.25f : 1f))
                },

                Sprite = GameResources.Sprites.Circle,
                sColor = selected.Map(x => x == index ? color : color.ColorWithAlpha(0.35f)),

                Do = x => x.WithPointerEvents(onDown: _ => selected.Value = index)
            };
        }

        #endregion
    }
}
