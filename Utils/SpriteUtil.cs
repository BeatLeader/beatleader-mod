using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    public class SpriteUtil
    {
        public static Sprite CreateSpriteFromEmbeddedResource(string path, int x, int y)
        {
            return Sprite.Create(CreateTextureFromEmbeddedResource(path, x, y), new Rect(0, 0, x, y), new Vector2(0.5f, 0.5f));
        }
        public static Texture2D CreateTextureFromEmbeddedResource(string path, int x, int y)
        {
            Texture2D texture = new Texture2D(x, y, TextureFormat.ARGB32, false);

            using Stream stream = ResourcesUtils.GetEmbeddedResourceStream(path);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, int.Parse($"{stream.Length}"));

            ImageConversion.LoadImage(texture, buffer);
            return texture;
        }
    }
}
