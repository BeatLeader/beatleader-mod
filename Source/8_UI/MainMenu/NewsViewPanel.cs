using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using BBillboard = BeatLeader.UI.Reactive.Components.Billboard;

namespace BeatLeader.UI.MainMenu;

internal class NewsViewPanel : ReactiveComponent {
    #region Public API

    //TODO: Oh my god please forgive me for this
    public INotifyValueChanged<PlatformEventStatus?> HappeningEvent => _eventDetailsStatus;

    private PlatformEvent? eventToAutoopen = null;

    public void SetEvents(RequestState state, IReadOnlyList<PlatformEvent>? events) {
        _eventNewsPanel.SetData(state, events);
        DanceEvent.SetEvents(events);
        if (DanceEvent.Current is { } dance && !_danceAutoOpened) {
            _danceAutoOpened = true;
            PresentDanceEvent(dance);
        }

        var happening = events?.FirstOrDefault(x => x.eventType is 1 && x.IsHappening());
        if (happening != null) {
            eventToAutoopen = happening;
            FetchEventStatus(happening);
        }
    }

    public void RefreshEventsCache() {
        _cachedEvents.Clear();

        if (_eventDetailsStatus.Value is { } status) {
            _eventDetailsStatus.Value = null;
            FetchEventStatus(status.eventDescription);
        }
    }

    #endregion

    #region Event Status

    private readonly Dictionary<string, PlatformEventStatus> _cachedEvents = new();
    private IWebRequest<PlatformEventStatus>? _statusRequest;

    private void HandleEventSelected(PlatformEvent evt) {
        if (evt.eventType == DanceEvent.EventType) {
            PresentDanceEvent(evt);
            return;
        }
        if (evt.eventType != 1) {
#pragma warning disable CS0618 // Type or member is obsolete
            ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, evt);
#pragma warning restore CS0618 // Type or member is obsolete
            return;
        }

        PresentEventDetails(evt);
    }

    private void PresentEventDetails(PlatformEvent evt) {
        _eventDetailsPresented.Value = true;

        FetchEventStatus(evt);
    }

    private void FetchEventStatus(PlatformEvent evt) {
        if (_cachedEvents.TryGetValue(evt.id, out var status)) {
            _eventDetailsStatus.Value = status;
        } else {
            if (_statusRequest is { RequestState: RequestState.Started }) {
                _statusRequest.StateChangedEvent -= HandleRequestUpdated;
                _statusRequest.Cancel();
            }

            _eventDetailsStatus.Value = null;
            _statusRequest = PlatformEventStatusRequest.Send(evt.id);
            _statusRequest.StateChangedEvent += HandleRequestUpdated;
        }
    }

    private void HandleRequestUpdated(IWebRequest<PlatformEventStatus> instance, RequestState state, string? failReason) {
        switch (state) {
            case RequestState.Finished:
                var result = instance.Result!;

                _eventDetailsStatus.Value = result;
                _cachedEvents.Add(result.eventDescription.id, result);
                if (eventToAutoopen?.id == result.eventDescription.id && result.today?.score == null) {
                    PresentEventDetails(eventToAutoopen);
                    eventToAutoopen = null;
                }
                break;
            case RequestState.Failed:
                _eventDetailsPresented.Value = false;
                break;
        }
    }

    #endregion

    #region Dance Event

    private static bool _danceAutoOpened;
    private IWebRequest<DanceEventStatus>? _danceRequest;
    private bool _danceHasData;

    private void PresentDanceEvent(PlatformEvent evt) {
        _dancePresented.Value = true;
        _dancePanel.SetEvent(evt);
        _dancePanel.SetLoading();
        _danceHasData = false;
        LoadDanceStatus(evt);
    }

    public void RefreshDanceEvent() {
        if (!_dancePresented.Value || DanceEvent.Current is not { } evt) return;
        LoadDanceStatus(evt);
    }

    private void LoadDanceStatus(PlatformEvent evt) {
        if (_danceRequest is { RequestState: RequestState.Started }) {
            _danceRequest.StateChangedEvent -= HandleDanceRequestUpdated;
            _danceRequest.Cancel();
        }
        _danceRequest = DanceEventStatusRequest.Send(evt.id);
        _danceRequest.StateChangedEvent += HandleDanceRequestUpdated;
    }

    private void HandleDanceRequestUpdated(IWebRequest<DanceEventStatus> instance, RequestState state, string? failReason) {
        switch (state) {
            case RequestState.Finished:
                _dancePanel.SetData(instance.Result!);
                _danceHasData = true;
                break;
            case RequestState.Failed:
                if (!_danceHasData) _dancePanel.SetFailed();
                break;
        }
    }

    #endregion

    #region Construct

    private EventNewsPanel _eventNewsPanel = null!;
    private ObservableValue<PlatformEventStatus?> _eventDetailsStatus = null!;
    private ObservableValue<bool> _eventDetailsPresented = null!;
    private DanceEventPanel _dancePanel = null!;
    private ObservableValue<bool> _dancePresented = null!;

    protected override GameObject Construct() {
        _eventDetailsStatus = Remember<PlatformEventStatus?>(null);
        _eventDetailsPresented = Remember(false);
        _dancePresented = Remember(false);

        var eventNews = new Layout {
                Children = {
                    new EventNewsPanel {
                        OnEventSelected = HandleEventSelected
                    }.Bind(ref _eventNewsPanel)
                    .AsFlexItem(size: new() { y = 12f }),

                    new MapNewsPanel(),
                }
            }
            .AsFlexGroup(direction: FlexDirection.Column, gap: 1f)
            .AsFlexItem(size: new() { x = 70f });

        var specialNews = new SpecialEventPanel {
                OnBackClick = () => _eventDetailsPresented.Value = false
            }
            .Animate(
                _eventDetailsStatus,
                (x, y) => {
                    if (y == null) {
                        x.SetLoading();
                    } else {
                        x.SetData(y);
                    }
                }
            );

        var danceNews = new DanceEventPanel {
                OnBackClick = () => _dancePresented.Value = false
            }
            .Bind(ref _dancePanel);

        return new Layout {
                Children = {
                    new TextNewsPanel(),

                    new BBillboard()
                        .AsFlexItem(flex: 1, size: new() { y = 70f })
                        .With(x => x.Push(eventNews))
                        .Animate(
                            _eventDetailsPresented,
                            (billboard, y) => {
                                if (y) {
                                    billboard.Push(specialNews);
                                    specialNews.PlayPreview();
                                } else {
                                    billboard.Pop();
                                    specialNews.CancelPreview();
                                }
                            }
                        )
                        .Animate(
                            _dancePresented,
                            (billboard, y) => {
                                if (y) {
                                    billboard.Push(danceNews);
                                    DanceEventPanel.Shown = danceNews.ContentTransform;
                                } else {
                                    billboard.Pop();
                                    DanceEventPanel.Shown = null;
                                }
                            }
                        )
                }
            }
            .AsFlexGroup(
                direction: FlexDirection.Row,
                constrainHorizontal: false,
                constrainVertical: false,
                gap: 1f,
                padding: new() { left = 30f }
            )
            .AsFlexItem()
            .Use();
    }

    #endregion
}