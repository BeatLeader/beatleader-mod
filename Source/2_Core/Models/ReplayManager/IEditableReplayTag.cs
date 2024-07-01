using UnityEngine;

namespace BeatLeader.Models {
    public interface IEditableReplayTag : IReplayTag {
        void SetColor(Color color);
        ReplayTagValidationResult SetName(string name);
    }
}