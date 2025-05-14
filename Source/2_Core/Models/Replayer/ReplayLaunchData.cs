using BeatLeader.Models.AbstractReplay;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    /// <summary>
    /// A data class with launch data for replayer.
    /// </summary>
    [PublicAPI]
    public class ReplayLaunchData {
        /// <summary>
        /// Specifies the list of replays. If there is more than one replay,
        /// replayer will be loaded in the battle royale mode.
        /// </summary>
        public IReadOnlyList<IReplay> Replays {
            get => _replays ?? throw Uninitialized();
            internal set => _replays = value;
        }

        /// <summary>
        /// Specifies a level and a key to load the beatmap. This field is required.
        /// </summary>
        public BeatmapLevelWithKey BeatmapLevel {
            get => _beatmapLevel ?? throw Uninitialized();
            internal set => _beatmapLevel = value;
        }

        /// <summary>
        /// Specifies a settings instance. This field is required.
        /// </summary>
        public ReplayerSettings Settings {
            get => _settings ?? throw Uninitialized();
            internal set => _settings = value;
        }

        /// <summary>
        /// Specifies an environment to be used instead of the default one.
        /// </summary>
        public EnvironmentInfoSO? EnvironmentInfo { get; internal set; }

        /// <summary>
        /// Specifies bindings to be used instead of the default ones.
        /// </summary>
        public ReplayerBindings? ReplayerBindings { get; set; }

        public IReplay MainReplay => Replays[0];
        public bool IsBattleRoyale => Replays.Count > 1;

        private IReadOnlyList<IReplay>? _replays;
        private BeatmapLevelWithKey? _beatmapLevel;
        private ReplayerSettings? _settings;

        public event Action<StandardLevelScenesTransitionSetupDataSO, ReplayLaunchData>? ReplayWasFinishedEvent;

        public void Init(
            IReplay replay,
            ReplayerSettings settings,
            BeatmapLevelWithKey beatmapLevel,
            EnvironmentInfoSO? environmentInfo = null,
            ReplayerBindings? bindings = null
        ) {
            Init(new[] { replay }, settings, beatmapLevel, environmentInfo, bindings);
        }

        public void Init(
            IReadOnlyList<IReplay> replays,
            ReplayerSettings settings,
            BeatmapLevelWithKey beatmapLevel,
            EnvironmentInfoSO? environmentInfo = null,
            ReplayerBindings? bindings = null
        ) {
            Replays = replays;
            BeatmapLevel = beatmapLevel;
            EnvironmentInfo = environmentInfo;
            ReplayerBindings = bindings;
            Settings = settings;
        }

        public void FinishReplay(StandardLevelScenesTransitionSetupDataSO transitionData) {
            ReplayWasFinishedEvent?.Invoke(transitionData, this);
        }

        public override string ToString() {
            var line = "ReplayerMode: ";

            if (IsBattleRoyale) {
                line += $"BattleRoyale\nPlayers Count: {Replays.Count}\n";
            } else {
                line += "Default\n";
            }

            line += MainReplay.ReplayData.ToString();

            return line;
        }

        private static Exception Uninitialized() {
            return new InvalidOperationException("The instance is not initialized");
        }
    }
}