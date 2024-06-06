﻿using System.Linq;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class BsPrimaryButton : ImageButton {
        private static readonly Material buttonMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "AnimatedButton");
        
        private static readonly Material buttonBorderMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "AnimatedButtonBorder");
        
        private static readonly Sprite roundRect10Border = Resources
            .FindObjectsOfTypeAll<Sprite>()
            .First(static x => x.name == "RoundRect10Border");

        private Image _borderImage = null!;

        protected override void ApplyColor(Color color) {
            Image.GradientColor0 = color;
            color.a /= 2f;
            Image.GradientColor1 = color;
            _borderImage.Color = Colors?.Color ?? color;
        }

        protected override void Construct(RectTransform rect) {
            //border
            new Image {
                Sprite = roundRect10Border,
                Material = buttonBorderMaterial,
                ImageType = UnityEngine.UI.Image.Type.Sliced
            }.WithRectExpand().Bind(ref _borderImage);
            base.Construct(rect);
            //applying later to move it over the content
            _borderImage.Use(rect);
        }

        protected override void OnInitialize() {
            Colors = new() {
                ActiveColor = new(0, 0.75f, 0.75f),
                HoveredColor = new(0, 0.75f, 1f),
                Color = UIStyle.ButtonColorSet.Color
            };
            GrowOnHover = false;
            HoverLerpMul = 100f;
            Image.Material = buttonMaterial;
            Image.Sprite = BundleLoader.Sprites.background;
            Image.PixelsPerUnit = 15f;
            Image.UseGradient = true;
            Image.GradientDirection = ImageView.GradientDirection.Vertical;
        }
    }
}