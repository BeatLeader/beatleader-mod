using BeatLeader.Replayer;
using System;
using System.Reflection;
using BeatLeader.Attributes;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Interop {
    [PluginInterop("Camera2")]
    internal static class Cam2Interop {
        #region Setup

        [PluginAssembly]
        private static readonly Assembly pluginAssembly = null!;

        [PluginType("Camera2.SDK.ReplaySources")]
        private static readonly Type replaySourcesType = null!;

        [PluginState]
        public static bool IsInitialized { get; private set; }

        private static object[] _cachedArgs = null!;
        private static PropertyInfo? _headPosProp;
        private static PropertyInfo? _headRotProp;
        private static MethodInfo? _setActiveMethod;
        private static object? _genericSourceInstance;

        [InteropEntry]
        private static void Init() {
            var genericSourceType = replaySourcesType.GetNestedType("GenericSource");
            var registerMethod = replaySourcesType.GetMethod("Register", ReflectionUtils.StaticFlags);
            _setActiveMethod = genericSourceType.GetMethod("SetActive", ReflectionUtils.DefaultFlags);

            _genericSourceInstance = Activator.CreateInstance(genericSourceType, "BeatLeaderReplayer");
            registerMethod?.Invoke(null, new[] { _genericSourceInstance });
            _cachedArgs = new object[1];

            _headPosProp = genericSourceType.GetProperty(
                "localHeadPosition",
                ReflectionUtils.DefaultFlags
            );
            _headRotProp = genericSourceType.GetProperty(
                "localHeadRotation",
                ReflectionUtils.DefaultFlags
            );

            InitHarmonyPatches();
            ReplayerLauncher.ReplayWasStartedEvent += HandleReplayWasStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += HandleReplayWasFinished;
        }

        private static void SetReplayState(bool state) {
            _cachedArgs[0] = state;
            _setActiveMethod?.Invoke(_genericSourceInstance, _cachedArgs);
        }

        #endregion

        #region Movement

        private class PoseReceiver : IVirtualPlayerPoseReceiver {
            public Vector3 Position { get; private set; }
            public Quaternion Rotation { get; private set; }

            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Position = headPose.position;
                Rotation = headPose.rotation;
            }
        }

        private static readonly PoseReceiver poseReceiver = new();
        private static IVirtualPlayerMovementProcessor? _movementProcessor;
        private static bool _hasBoundProcessor;

        public static void BindMovementProcessor(IVirtualPlayerMovementProcessor processor) {
            processor.AddListener(poseReceiver);
            _movementProcessor = processor;
            _hasBoundProcessor = true;
        }

        public static void UnbindMovementProcessor() {
            if (!_hasBoundProcessor) return;
            _movementProcessor!.RemoveListener(poseReceiver);
            _hasBoundProcessor = false;
        }

        #endregion

        #region Harmony Patches

        private static HarmonyAutoPatch? _headPositionPropertyPatch;
        private static HarmonyAutoPatch? _headRotationPropertyPatch;

        private static void LocalHeadPositionPrefix(object __instance) {
            if (!_hasBoundProcessor || _genericSourceInstance != __instance) return;
            _headPosProp?.SetValue(__instance, poseReceiver.Position);
        }

        private static void LocalHeadRotationPrefix(object __instance) {
            if (!_hasBoundProcessor || _genericSourceInstance != __instance) return;
            _headRotProp?.SetValue(__instance, poseReceiver.Rotation);
        }

        private static void InitHarmonyPatches() {
            _headPositionPropertyPatch = new(
                new(
                    _headPosProp!.GetGetMethod(),
                    typeof(Cam2Interop).GetMethod(
                        nameof(LocalHeadPositionPrefix),
                        ReflectionUtils.StaticFlags
                    )
                )
            );
            _headRotationPropertyPatch = new(
                new(
                    _headRotProp!.GetGetMethod(),
                    typeof(Cam2Interop).GetMethod(
                        nameof(LocalHeadRotationPrefix),
                        ReflectionUtils.StaticFlags
                    )
                )
            );
        }

        #endregion

        #region Callbacks

        private static void HandleReplayWasStarted(ReplayLaunchData data) {
            if (InputUtils.IsInFPFC) {
                ReplayerLauncher.LaunchData!.Settings.CameraSettings = null; //disabling base camera
            }
            SetReplayState(true);
        }

        private static void HandleReplayWasFinished(ReplayLaunchData data) {
            SetReplayState(false);
        }

        #endregion
    }
}