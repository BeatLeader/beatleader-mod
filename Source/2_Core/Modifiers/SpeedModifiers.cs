using ModifiersCore;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader {
    public static class SpeedModifiers {
        public static CustomModifier BFS = new CustomModifier(
            "BFS",
            "Better Faster Song",
            "Increases song speed by 20%, note speed by 10%",
            BundleLoader.BFSIcon,
            Color.blue,
            Color.yellow,
            0.1f,
            new string[] { ModifierCategories.Speed },
            new string[] { ModifierCategories.Speed }
        );

        public static CustomModifier BSF = new CustomModifier(
            "BSF",
            "Better Super Fast Song",
            "Increases song speed by 50%, note speed by 25%",
            BundleLoader.BSFIcon,
            Color.blue,
            Color.yellow,
            0.2f,
            new string[] { ModifierCategories.Speed },
            new string[] { ModifierCategories.Speed }
        );

        public static GameplayModifiers.SongSpeed GetSongSpeed() {
            if (ModifiersManager.GetModifierState(BFS.Id)) {
                return GameplayModifiers.SongSpeed.Faster;
            }
            if (ModifiersManager.GetModifierState(BSF.Id)) {
                return GameplayModifiers.SongSpeed.SuperFast;
            }
            return GameplayModifiers.SongSpeed.Normal;
        }
        
        public static float GetSongSpeedMultiplier() {
            if (ModifiersManager.GetModifierState(BFS.Id)) {
                return 1.2f;
            }
            if (ModifiersManager.GetModifierState(BSF.Id)) {
                return 1.5f;
            }
            return 1f;
        }
    }
}