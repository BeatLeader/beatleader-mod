using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace BeatLeader {
    internal class HarmonyMultisilencer : IDisposable {
        public HarmonyMultisilencer(IEnumerable<MethodInfo> methods = null, bool enable = true) {
            silencedMethods = new(methods);
            silencedMethods.CollectionChanged += HandleObservableCollectionChanged;
            Enabled = enable;
            HandleObservableCollectionChanged(null, null);
        }

        public bool Enabled {
            get => _atLeastOneSilencerIsEnabled;
            set {
                _atLeastOneSilencerIsEnabled = value;
                _silencers.ForEach(x => x.Enabled = value);
            }
        }

        public readonly ObservableCollection<MethodInfo> silencedMethods = new();
        private readonly List<HarmonySilencer> _silencers = new();
        private bool _atLeastOneSilencerIsEnabled;

        public void Dispose() {
            _silencers.ForEach(x => x.Dispose());
            silencedMethods.Clear();
        }

        private void HandleObservableCollectionChanged(object sender, EventArgs e) {
            _silencers.ToList().ForEach(x => {
                if (!silencedMethods.Contains(x.method)) {
                    _silencers.Remove(x);
                }
            });

            var silenceList = silencedMethods.ToList();
            var selectedSilencers = _silencers.Select(x => x.method);

            foreach (var item in silencedMethods) {
                if (selectedSilencers.Contains(item)) {
                    silenceList.Remove(item);
                }
            };

            silenceList.ForEach(x => _silencers.Add(new(x, Enabled)));
        }
    }
}
