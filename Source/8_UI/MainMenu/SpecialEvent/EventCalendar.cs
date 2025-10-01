using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using Reactive;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventCalendar : ReactiveComponent {
        #region Public API

        public Action<PlatformEventMap>? OnDayChanged { get; set; }

        private Color _accent;

        public void SetData(PlatformEventStatus status) {
            PlatformEventMap[] maps;
            DateTime lastMentionedDay;

            if (status.today == null) {
                maps = status.previousDays;
                lastMentionedDay = status.previousDays.Max(x => x.StartDate());
            } else {
                lastMentionedDay = status.today.StartDate().Date;
                maps = [..status.previousDays, status.today];
            }

            var endDate = status.eventDescription.EndDate().Date;
            var remainingDays = Mathf.Abs((endDate - lastMentionedDay).Days) - 1;

            _accent = status.eventDescription.MainColor() ?? Color.cyan * 0.7f;

            DespawnCells();
            SpawnCells(maps, remainingDays);
        }

        private void NotifyDayChanged(EventCalendarCell cell) {
            if (!_cellsToMaps.TryGetValue(cell, out var map)) {
                return;
            }

            if (_selectedCell != null) {
                _selectedCell.Selected = false;
            }
            _selectedCell = cell;
            cell.Selected = true;

            OnDayChanged?.Invoke(map);
        }

        #endregion

        #region Construct

        private readonly ReactivePool<EventCalendarCell> _cellsPool = new() { DetachOnDespawn = false };
        private readonly Dictionary<EventCalendarCell, PlatformEventMap> _cellsToMaps = new();
        private EventCalendarCell? _selectedCell;
        private Layout _content = null!;

        private void SpawnCells(IEnumerable<PlatformEventMap> maps, int remainingDays) {
            DateTime lastDate = default;

            foreach (var map in maps) {
                lastDate = map.StartDate();
                SpawnCell(map.StartDate(), map.IsCompleted(), map);
            }

            for (var i = 0; i < remainingDays; i++) {
                var date = lastDate.AddDays(i + 1);
                SpawnCell(date, false, null);
            }
        }

        private void SpawnCell(DateTime date, bool completed, PlatformEventMap? map) {
            var cell = _cellsPool.Spawn();

            cell.Completed = completed;
            cell.Accent = _accent;
            cell.Date = date;

            if (map != null) {
                var selected = map.IsHappening();
                cell.Selected = selected;
                cell.OnClick ??= NotifyDayChanged;

                if (selected) {
                    _selectedCell = cell;
                }
                _cellsToMaps.Add(cell, map);
            } else {
                cell.Selected = false;
            }

            _content.Children.Add(cell);
        }

        private void DespawnCells() {
            _selectedCell = null;
            _cellsToMaps.Clear();

            _content.Children.Clear();
            _cellsPool.DespawnAll();
        }

        protected override GameObject Construct() {
            return new Layout()
                .AsFlexGroup(gap: 1.pt(), wrap: Wrap.Wrap)
                .AsFlexItem()
                .Bind(ref _content)
                .Use();
        }

        #endregion
    }
}