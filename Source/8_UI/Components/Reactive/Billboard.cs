using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;
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

        comp.RecalculateLayoutImmediate();
        _outerWidth.Value = comp.ContentTransform.rect.width;

        RecalculatePos();
    }

    public void Pop() {
        if (_components.Count == 0) {
            return;
        }

        var comp = _components.Pop();
        _pendingRemoval.Add(comp);
        RecalculatePos();

        if (_components.TryPeek(out var peek)) {
            _outerWidth.Value = peek!.ContentTransform.rect.width;
        } else {
            _outerWidth.Value = 0f;
        }
    }

    private void RecalculatePos() {
        _innerPosX.Value = -_components
            .Skip(1)
            .Sum(x => {
                    x.RecalculateLayoutImmediate();
                    return x.ContentTransform.rect.width;
                }
            );
    }

    private void HandlePosAnimationFinished(AnimatedValue<float> _) {
        foreach (var comp in _pendingRemoval) {
            _inner.Children.Remove(comp);
        }

        _pendingRemoval.Clear();
    }

    #endregion

    #region Construct

    private Layout _inner = null!;
    private YogaModifier _outerModifier = null!;

    private AnimatedValue<float> _innerPosX = null!;
    private AnimatedValue<float> _outerWidth = null!;

    protected override GameObject Construct() {
        _innerPosX = RememberAnimated(0f, 300.ms(), AnimationCurve.EaseInOut, HandlePosAnimationFinished);
        _outerWidth = RememberAnimated(0f, 300.ms(), AnimationCurve.EaseInOut);

        return new Layout {
                ContentTransform = {
                    pivot = new(0f, 0f)
                },

                Children = {
                    new Layout {
                            ContentTransform = {
                                anchorMin = new(0f, 0f),
                                anchorMax = new(0f, 1f),
                                pivot = new(0f, 0f)
                            }
                        }
                        .Animate(_innerPosX, (x, y) => x.ContentTransform.localPosition = new(y, 0f))
                        .AsFlexGroup(direction: FlexDirection.Row, constrainHorizontal: false)
                        .Bind(ref _inner)
                }
            }
            .WithNativeComponent(out RectMask2D _)
            .AsFlexItem(modifier: out _outerModifier)
            .Animate(_outerWidth, (_, y) => _outerModifier.Size = new() { x = y })
            .Use();
    }

    #endregion
}