using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class EventCalendar : ReactiveComponent {
        #region Public API

        private Color _accent;

        public void SetData(PlatformEventStatus status) {
            int remainingDays;
            PlatformEventMap[] maps;

            if (status.today == null) {
                remainingDays = 0;
                maps = status.previousDays;
            } else {
                var startDate = status.today.EndDate().Date;
                var endDate = status.eventDescription.EndDate().Date;

                remainingDays = Mathf.Abs((endDate - startDate).Days);
                maps = [..status.previousDays, status.today];
            }

            _accent = status.eventDescription.MainColor() ?? Color.cyan * 0.7f;

            DespawnCells();
            SpawnCells(maps, remainingDays);
        }

        #endregion

        #region Construct

        private readonly ReactivePool<EventCalendarCell> _cellsPool = new() { DetachOnDespawn = false };
        private Layout _content = null!;

        private void SpawnCells(IEnumerable<PlatformEventMap> maps, int remainingDays) {
            DateTime lastDate = default;

            foreach (var map in maps) {
                lastDate = map.EndDate();
                SpawnCell(map.EndDate(), map.IsCompleted());
            }

            for (var i = 0; i < remainingDays; i++) {
                var date = lastDate.AddDays(i + 1);
                SpawnCell(date, false);
            }
        }

        private void SpawnCell(DateTime date, bool completed) {
            var cell = _cellsPool.Spawn();

            cell.Completed = completed;
            cell.Accent = _accent;
            cell.Date = date;

            _content.Children.Add(cell);
        }

        private void DespawnCells() {
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