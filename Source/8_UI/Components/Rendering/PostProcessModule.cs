using UnityEngine;

namespace BeatLeader.UI.Rendering {
    internal abstract class PostProcessModule : MonoBehaviour {
        public abstract void Process(RenderTexture texture);
    }
}