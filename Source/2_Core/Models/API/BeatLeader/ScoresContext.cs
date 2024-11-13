using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Models {
    public class ScoresContext {
        public int Id { get; set; }
        public Sprite Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }

    public static class ScoresContexts {
        public static ScoresContext General = new ScoresContext {
            Id = 0,
            Icon = BundleLoader.GeneralContextIcon,
            Name = "General",
            Description = "General",
            Key = "modifiers"
        };
        public static List<ScoresContext> AllContexts = new List<ScoresContext> { General };
        public static ScoresContext ContextForId(int id) => AllContexts.FirstOrDefault(c => c.Id == id) ?? General;
    }
}