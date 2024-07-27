using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.Models {
    [JsonConverter(typeof(TypeHandlingConverter))]
    public interface ICameraView {
        string Name { get; }
        
        Pose ProcessPose(Pose headPose);
        
        void OnEnable();
        void OnDisable();
    }
}
