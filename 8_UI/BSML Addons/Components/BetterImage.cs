using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
