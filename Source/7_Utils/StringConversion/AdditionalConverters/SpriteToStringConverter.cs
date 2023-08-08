using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader {
    public class SpriteToStringConverter : StringConverter<Sprite> {
        protected override Sprite? ConvertTyped(string str) {
            try {
                return BSMLUtility.LoadSprite(str);
            } catch (Exception) {
                return null;
            }
        }
    }
}