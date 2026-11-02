using System.Collections.Generic;
using BeatLeader.Interop;
using UnityEngine;

namespace BeatLeader.Replayer {
    internal static class VivifyPrefabsCatchUp {
        private const float MaxSimulationWindow = 30f;
        private const float TrailSimulationRate = 60f;
        private const float SimulationRate = 20f;
        private const int MaxSimulationSteps = 900;
        private const float SimulationBudgetMs = 10f;
        private const float StepCostMs = 0.01f;
        private const float ParticleStepCostMs = 8e-5f;
        private const float AnimatorStep = 0.25f;
        private const int MaxCoarseAnimatorSteps = 100;

        private class ParticleTree {
            public readonly ParticleSystem root;
            public readonly float duration;
            public readonly float rate;
            public readonly bool followsEmitter;
            private readonly float _stepCost;
            public float? enableTime;
            public bool started;
            public int substeps = 1;

            public ParticleTree(ParticleSystem root, float songTime) {
                this.root = root;
                var maxParticles = 0;
                var hasTrails = false;
                foreach (var system in root.GetComponentsInChildren<ParticleSystem>(true)) {
                    var main = system.main;
                    if (VivifyInterop.SetSyncTime(system, songTime)) main.simulationSpeed = 1f;
                    var lifetime = main.startLifetime;
                    var maxLifetime = lifetime.mode is ParticleSystemCurveMode.Constant or ParticleSystemCurveMode.TwoConstants
                        ? lifetime.constantMax
                        : lifetime.curveMultiplier;
                    duration = Mathf.Max(duration, maxLifetime / Mathf.Max(main.simulationSpeed, 0.01f));
                    hasTrails |= system.trails.enabled;
                    followsEmitter |= main.simulationSpace != ParticleSystemSimulationSpace.Local || system.trails.enabled && system.trails.worldSpace;
                    maxParticles += main.maxParticles;
                }
                rate = hasTrails ? TrailSimulationRate : SimulationRate;
                _stepCost = StepCostMs + maxParticles * ParticleStepCostMs;
            }

            public int GetSteps(float window) {
                var steps = Mathf.Min(window * rate, SimulationBudgetMs / _stepCost);
                return Mathf.Clamp(Mathf.CeilToInt(steps), 1, MaxSimulationSteps);
            }
        }

        public static List<ParticleSystem> CatchUp(List<GameObject> prefabs, float songTime) {
            var particles = new List<ParticleSystem>();
            foreach (var prefab in prefabs) {
                if (!prefab.activeInHierarchy || !VivifyInterop.TryGetSyncTime(prefab, out var startTime) || startTime >= songTime) continue;
                CatchUp(prefab, startTime, songTime, particles);
            }
            return particles;
        }

        public static void Resume(List<ParticleSystem> particles, float deltaTime) {
            var steps = Mathf.CeilToInt(Mathf.Max(deltaTime, 0f) * TrailSimulationRate);
            foreach (var system in particles) {
                if (system == null) continue;
                for (var i = 0; i < steps; i++) {
                    Simulate(system, deltaTime / steps, false);
                }
                system.Play(true);
            }
        }

        private static void CatchUp(GameObject prefab, float startTime, float songTime, List<ParticleSystem> particles) {
            var trees = new List<ParticleTree>();
            foreach (var system in prefab.GetComponentsInChildren<ParticleSystem>(true)) {
                var parent = system.transform.parent;
                if (parent != null && parent.GetComponentInParent<ParticleSystem>(true) != null) continue;
                if (!system.isPlaying && !system.main.playOnAwake) continue;
                trees.Add(new ParticleTree(system, songTime) {
                    enableTime = system.gameObject.activeInHierarchy ? startTime : null
                });
            }
            if (trees.Count == 0) return;

            var animators = prefab.GetComponentsInChildren<Animator>(true);
            if (animators.Length > 0) CatchUpAnimators(animators, trees, startTime, songTime);

            foreach (var tree in trees) {
                if (tree.enableTime is not { } enableTime) continue;
                if (!tree.started) SimulateWindow(tree, songTime - enableTime);
                particles.Add(tree.root);
            }
        }

        private static void CatchUpAnimators(Animator[] animators, List<ParticleTree> trees, float startTime, float songTime) {
            foreach (var animator in animators) {
                if (VivifyInterop.SetSyncTime(animator, songTime)) animator.speed = 1f;
            }

            var fineWindow = 0f;
            foreach (var tree in trees) {
                if (tree.followsEmitter) fineWindow = Mathf.Max(fineWindow, tree.duration);
            }
            fineWindow = Mathf.Min(fineWindow, MaxSimulationWindow, songTime - startTime);
            var fineStart = songTime - fineWindow;

            var coarseSpan = fineStart - startTime;
            if (coarseSpan > 0f) {
                var steps = Mathf.Clamp(Mathf.CeilToInt(coarseSpan / AnimatorStep), 1, MaxCoarseAnimatorSteps);
                var stepTime = coarseSpan / steps;
                for (var i = 1; i <= steps; i++) {
                    StepAnimators(animators, trees, stepTime, startTime + i * stepTime);
                }
            }
            if (fineWindow > 0f) {
                var steps = Mathf.CeilToInt(fineWindow / AnimatorStep);
                var stepTime = fineWindow / steps;
                foreach (var tree in trees) {
                    tree.substeps = Mathf.Clamp(tree.GetSteps(fineWindow) / steps, 1, Mathf.CeilToInt(stepTime * tree.rate));
                }
                for (var i = 0; i < steps; i++) {
                    var time = fineStart + i * stepTime;
                    foreach (var tree in trees) {
                        if (!tree.followsEmitter || tree.started || tree.enableTime is not { } enableTime) continue;
                        Simulate(tree.root, time - enableTime, true);
                        tree.started = true;
                    }
                    StepAnimators(animators, trees, stepTime, time + stepTime);
                    foreach (var tree in trees) {
                        if (!tree.started) continue;
                        for (var j = 0; j < tree.substeps; j++) {
                            Simulate(tree.root, stepTime / tree.substeps, false);
                        }
                    }
                }
            }
        }

        private static void StepAnimators(Animator[] animators, List<ParticleTree> trees, float stepTime, float time) {
            foreach (var animator in animators) {
                if (animator.isActiveAndEnabled) animator.Update(stepTime);
            }
            foreach (var tree in trees) {
                if (tree.root.gameObject.activeInHierarchy) {
                    tree.enableTime ??= time - stepTime / 2f;
                } else {
                    tree.enableTime = null;
                    tree.started = false;
                }
            }
        }

        private static void SimulateWindow(ParticleTree tree, float age) {
            var window = Mathf.Min(age, tree.duration, MaxSimulationWindow);
            var steps = tree.GetSteps(window);
            Simulate(tree.root, age - window, true);
            for (var i = 0; i < steps; i++) {
                Simulate(tree.root, window / steps, false);
            }
        }

        // Otherwise Unity splits the time into maximumParticleDeltaTime steps, which makes every long step as expensive as many short ones
        private static void Simulate(ParticleSystem system, float time, bool restart) {
            var maxDeltaTime = Time.maximumParticleDeltaTime;
            Time.maximumParticleDeltaTime = Mathf.Max(time, maxDeltaTime);
            try {
                system.Simulate(time, true, restart, false);
            } finally {
                Time.maximumParticleDeltaTime = maxDeltaTime;
            }
        }
    }
}
