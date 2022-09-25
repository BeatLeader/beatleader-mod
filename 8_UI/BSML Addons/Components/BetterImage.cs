using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons.Components
{
    public class BetterImage : MonoBehaviour
    {
        public Image Image { get; protected set; }

        public void Init(Image image)
        {
            Image = image;
        }
    }
}
