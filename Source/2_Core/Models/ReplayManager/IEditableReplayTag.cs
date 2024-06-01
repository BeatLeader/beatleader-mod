using UnityEngine;

namespace BeatLeader.Models {
    public interface IEditableReplayTag : IReplayTag {
        new Color Color { set; }

        ReplayTagValidationResult SetName(string name);
        void Delete();
    }
}