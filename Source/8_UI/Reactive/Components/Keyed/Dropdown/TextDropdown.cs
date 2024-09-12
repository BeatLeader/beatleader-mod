using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextDropdown<TKey> : Dropdown<TKey, string, TextDropdown<TKey>.ComponentCell> {
        #region Cell

        public class ComponentCell : KeyedControlComponentCell<TKey, string>, ISkewedComponent, IPreviewableCell {
            #region Setup

            public bool UsedAsPreview {
                set {
                    //TODO: replace with RaycastTarget
                    _button.Interactable = !value;
                    _label.RaycastTarget = !value;
                }
            }
            
            public float Skew {
                get => throw new NotImplementedException();
                set {
                    _label.FontStyle = value > 0f ? FontStyles.Italic : FontStyles.Normal;
                    _button.Image.Skew = value;
                }
            }

            public override void OnInit(TKey item, string text) {
                _label.Text = text;
            }

            #endregion

            #region Construct

            private Label _label = null!;
            private ImageButton _button = null!;

            protected override GameObject Construct() {
                return new ImageButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.rectangle,
                        Material = GameResources.UINoGlowMaterial
                    },
                    Colors = new SimpleColorSet {
                        HoveredColor = UIStyle.ControlColorSet.HoveredColor,
                        Color = Color.clear
                    },
                    OnClick = SelectSelf,
                    Children = {
                        new Label().WithRectExpand().Bind(ref _label)
                    }
                }.Bind(ref _button).Use();
            }

            #endregion

            #region Callbacks

            public override void OnCellStateChange(bool selected) {
                _label.Color = selected ? UIStyle.SelectedTextColor : UIStyle.TextColor;
            }

            #endregion
        }

        #endregion
    }
}