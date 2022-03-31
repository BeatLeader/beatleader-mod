using System.Collections;
using UnityEngine;

namespace BeatLeader.Models {
    public struct AvatarImage {
        public Texture Texture;
        public IEnumerator PlaybackCoroutine;
    }
}