using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    public class ScoresContext {
        public int Id { get; set; }
        public Sprite Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }

    [PublicAPI]
    public static class ScoresContexts {
        public static readonly ScoresContext General = new ScoresContext {
            Id = 0,
            Icon = BundleLoader.GeneralContextIcon,
            Name = "General",
            Description = "General",
            Key = "modifiers"
        };

        public static IReadOnlyList<ScoresContext> AllContexts => allContexts;
        
        internal static ScoresContext[] allContexts = [General];
        
        public static ScoresContext ContextForId(int id) {
            return allContexts.FirstOrDefault(c => c.Id == id) ?? General;
        }
    }
}