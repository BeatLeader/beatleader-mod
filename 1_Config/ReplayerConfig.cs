using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BeatLeader
{
    public class ReplayerConfig
    {
        public const string _path = @"UserData\BeatLeader\ReplayerConfig.json";
        public static ReplayerConfig instance = new();

        public Dictionary<string, bool> UIComponents = new();
        public bool MovementLerp { get; set; } = true;
        public string CameraView { get; set; } = "PlayerView";
        public int CameraFOV { get; set; } = 110;

        public static void Save()
        {
            if (!File.Exists(_path)) File.Create(_path);
            File.WriteAllText(_path, JsonConvert.SerializeObject(instance));
        }
        public static void Load()
        {
            if (File.Exists(_path))
                instance = JsonConvert.DeserializeObject<ReplayerConfig>(File.ReadAllText(_path));
            else File.Create(_path);
        }
    }
}
