using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Reactive;
using UnityEngine;
using JetBrains.Annotations;
using Reactive.Yoga;
using BBillboard = BeatLeader.UI.Reactive.Components.Billboard;

namespace BeatLeader.Components;

/// <summary>
/// Used to launch and setup replays. Provides an API to add extra buttons for another mods.
/// </summary>
[PublicAPI]
public class ReplayPanel : ReactiveComponent {
    #region Extra Buttons

    private static event Action<ReplayPanelExtraButton>? ButtonAddedEvent;
    private static event Action<ReplayPanelExtraButton>? ButtonRemovedEvent;

    private static readonly HashSet<ReplayPanelExtraButton> extraButtons = new();

    /// <summary>
    /// Adds an extra button to all instances of the panel.
    /// </summary>
    public static void AddExtraButton(ReplayPanelExtraButton extraButton) {
        extraButtons.Add(extraButton);
        ButtonAddedEvent?.Invoke(extraButton);
    }

    /// <summary>
    /// Adds an extra button from all instances of the panel.
    /// </summary>
    public static void RemoveExtraButton(ReplayPanelExtraButton extraButton) {
        extraButtons.Remove(extraButton);
        ButtonRemovedEvent?.Invoke(extraButton);
    }

    #endregion

    #region Public API

    public Action<bool>? OnCanCloseChanged { get; set; }
    public required ReplayerViewNavigatorWrapper Navigator { get; set; }

    private Score? _score;

    public void SetScore(Score? score) {
        _score = score;
    }

    #endregion

    #region Download

    private TaskGate _taskGate = new();

    private async Task DownloadScore() {
        if (_score == null) {
            throw new InvalidOperationException("Score is null");
        }

        var req = DownloadReplayRequest.SendRequest(_score.replay, _taskGate.Token);
        req.ProgressChangedEvent += (_, _, _, t) => {
            _downloadView.Progress = t;
        };
        
        await req.Join();
        _taskGate.ThrowIfCancellationRequested();

        await ReplayManager.SaveAnyReplayAsync(req.Result!, null, _taskGate.Token);
    }

    #endregion

    #region Construct

    private BBillboard _billboard = null!;
    private ReplayPanelMainView _mainView = null!;
    private ReplayPanelDownloadView _downloadView = null!;

    protected override GameObject Construct() {
        _mainView = new() {
            OnDownloadClicked = () => {
                OnCanCloseChanged?.Invoke(false);

                _taskGate.SetTask(DownloadScore().RunCatching());
                _billboard.Push(_downloadView);
            },

            OnPlayClicked = () => { }
        };

        _downloadView = new() {
            OnDownloadFinished = () => {
                OnCanCloseChanged?.Invoke(true);
            }
        };

        return new BBillboard()
            .AsFlexItem(size: new() { x = 50.pt() })
            .Bind(ref _billboard)
            .Use();
    }

    protected override void OnInitialize() {
        ButtonAddedEvent += _mainView.AddExtraButton;
        ButtonRemovedEvent += _mainView.RemoveExtraButton;
    }

    protected override void OnDestroy() {
        ButtonAddedEvent -= _mainView.AddExtraButton;
        ButtonRemovedEvent -= _mainView.RemoveExtraButton;
    }

    #endregion
}