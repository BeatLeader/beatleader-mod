using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
            GC.KeepAlive(instance); //love that fix xD
            if (!File.Exists(_path)) File.Create(_path);
            File.WriteAllText(_path, JsonConvert.SerializeObject(instance));
        }
        public static void Load()
        {
            if (File.Exists(_path))
            {
                var config = JsonConvert.DeserializeObject<ReplayerConfig>(File.ReadAllText(_path));
                instance = config != null ? config : new();
            }
            else File.Create(_path);
        }
    }
}
