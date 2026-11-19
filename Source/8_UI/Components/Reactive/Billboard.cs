using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Reactive.Components;

[PublicAPI]
public class Billboard : ReactiveComponent {
    #region Public API

    public AnimationDuration AnimationDuration {
        get => _outerWidth.Duration;
        set {
            _outerWidth.Duration = value;
            _innerPosX.Duration = value;
        }
    }

    public AnimationCurve AnimationCurve {
        get => _outerWidth.Curve;
        set {
            _outerWidth.Curve = value;
            _innerPosX.Curve = value;
        }
    }

    private readonly Stack<IReactiveComponent> _components = new();
    private readonly HashSet<IReactiveComponent> _pendingRemoval = new();

    public void Push(IReactiveComponent comp) {
        // Add to children only if not already there
        if (!_pendingRemoval.Remove(comp)) {
            _inner.Children.Add(comp);
        }
        _components.Push(comp);

        RefreshTargets();
    }

    public void Pop() {
        if (_components.Count == 0) {
            return;
        }

        _pendingRemoval.Add(_components.Pop());

        RefreshTargets();
    }

    private void RefreshTargets() {
        _inner.RecalculateLayout();

        _outerWidth.TargetValue = _components.TryPeek(out var top) ? top!.ContentTransform.rect.width : 0f;

        _innerPosX.TargetValue = -_components
            .Skip(1)
            .Sum(x => x.ContentTransform.rect.width);
    }

    private void HandlePosAnimationFinished(float _) {
        foreach (var comp in _pendingRemoval) {
            _inner.Children.Remove(comp);
        }

        _pendingRemoval.Clear();
    }

    #endregion

    #region Construct

    private Layout _inner = null!;

    private AnimatedState<float> _innerPosX = null!;
    private AnimatedState<float> _outerWidth = null!;

    protected override GameObject Construct() {
        _innerPosX = RememberAnimated(0f, 300.ms, AnimationCurve.EaseInOut, HandlePosAnimationFinished);
        _outerWidth = RememberAnimated(0f, 300.ms, AnimationCurve.EaseInOut);

        return new Layout {
            FlexItem = {
                sSize = _outerWidth.Map(x => new YogaVector { x = x }).InLazy()
            },

            ContentTransform = {
                pivot = new(0f, 0f)
            },

            Children = {
                new Layout {
                    FlexController = {
                        FlexDirection = FlexDirection.Row,
                        ConstrainHorizontal = false
                    },

                    ContentTransform = {
                        anchorMin = new(0f, 0f),
                        anchorMax = new(0f, 1f),
                        pivot = new(0f, 0f),
                        slocalPosition = _innerPosX.Map(x => new Vector3(x, 0f)).InLazy()
                    }
                }.Bind(ref _inner)
            }
        }.AsRectMask().Use();
    }

    #endregion
}