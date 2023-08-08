using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader {
    public class StringToSpriteConverter : StringConverter<Sprite> {
        protected override Sprite? ConvertTo(string str) {
            try {
                return BSMLUtility.LoadSprite(str);
            } catch (Exception) {
                return null;
            }
        }
    }
}