using BeatLeader.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace BeatLeader.Replayer.Tweaking
{
    internal class MethodsSilencerTweak : GameTweak
    {
        private static readonly IReadOnlyList<MethodInfo> silencedMethods = new[]
        {
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.ShowMenu), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.Update), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.StartResumeAnimation), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.RestartButtonPressed), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.ContinueButtonPressed), ReflectionUtils.DefaultFlags),

            typeof(StandardLevelGameplayManager).GetMethod(nameof(StandardLevelGameplayManager.Update), ReflectionUtils.DefaultFlags),
            typeof(CuttingManager).GetMethod(nameof(CuttingManager.HandleSaberManagerDidUpdateSaberPositions), ReflectionUtils.DefaultFlags)
        };

        private HarmonyMultisilencer _multisilencer;

        public override void Initialize()
        {
            _multisilencer = new(silencedMethods);
        }
        public override void Dispose()
        {
            _multisilencer.Dispose();
        }
    }
}
