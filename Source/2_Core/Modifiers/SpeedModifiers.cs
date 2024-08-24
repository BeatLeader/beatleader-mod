using ModifiersCore;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader {
    public static class SpeedModifiers {
        public static CustomModifier BFS = new CustomModifier(
            "BFS", 
            "Better Faster Song",
            "Increases song speed by 20%, note speed by 10%",
            BundleLoader.AnchorIcon,
            Color.blue,
            Color.yellow,
            0.1f,
            new string[] { DefaultCategory.SPEED },
            new string[] { DefaultCategory.SPEED }
            );
        public static CustomModifier BSF = new CustomModifier(
            "BSF", 
            "Better Super Fast Song",
            "Increases song speed by 50%, note speed by 25%",
            BundleLoader.AnchorIcon,
            Color.blue,
            Color.yellow,
            0.2f,
            new string[] { DefaultCategory.SPEED },
            new string[] { DefaultCategory.SPEED }
            );

        public static float SongSpeed() {
            if (ModifiersManager.GetModifierState(BFS.Id))
            {
                return 1.2f;
            }
            else if (ModifiersManager.GetModifierState(BSF.Id))
            {
                return 1.5f;
            }
            return 1f;
        }
    }
}
