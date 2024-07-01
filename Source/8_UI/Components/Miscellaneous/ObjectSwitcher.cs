using System.Collections.Generic;
using BeatLeader.Utils;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    /// <summary>
    /// Container with ability to easily switch shown objects inside it
    /// </summary>
    internal class ObjectSwitcher : LayoutComponentBase<ObjectSwitcher> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        private IList<KeyValuePair<string, int>> Bindings {
            set => value.ForEach(x => bindings.Add(x.Key, x.Value));
        }
        
        public readonly Dictionary<string, int> bindings = new();
            
        #endregion
        
        #region ShowObjectWithKey
        
        /// <summary>
        /// Shows only object with the specified key
        /// </summary>
        /// <param name="key">Object key</param>
        public void ShowObjectWithKey(string key) {
            var childIdx = bindings[key];
            ShowObjectWithIndex(childIdx);
        }

        public void ShowObjectWithIndex(int childIdx) {
            for (var i = 0; i < ContentTransform!.childCount; i++) {
                ContentTransform!.GetChild(i).gameObject.SetActive(i == childIdx);
            }
        }

        #endregion
    }
}