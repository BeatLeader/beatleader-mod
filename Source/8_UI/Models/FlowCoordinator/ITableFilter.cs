using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.UI.Hub {
    internal interface ITableFilter<in T> {
        event Action? FilterUpdatedEvent;

        bool Matches(T value);
    }

    internal class CompositeTableFilter<T> : ITableFilter<T> {
        public CompositeTableFilter() {
            _filters = new(
                HandleFilterAdded,
                HandleFilterRemoved,
                HandleAllFiltersRemoved
            );
        }

        public ICollection<ITableFilter<T>> Filters => _filters;

        public event Action? FilterUpdatedEvent;

        private readonly ObservableSet<ITableFilter<T>> _filters;

        public bool Matches(T value) {
            return _filters.All(x => x.Matches(value));
        }

        private void HandleFilterAdded(ITableFilter<T> filter) {
            filter.FilterUpdatedEvent += HandleFilterUpdated;
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleFilterRemoved(ITableFilter<T> filter) {
            filter.FilterUpdatedEvent -= HandleFilterUpdated;
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleAllFiltersRemoved(IEnumerable<ITableFilter<T>> filters) {
            foreach (var filter in filters) {
                filter.FilterUpdatedEvent -= HandleFilterUpdated;
            }
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleFilterUpdated() {
            FilterUpdatedEvent?.Invoke();
        }
    }

    internal static class TableFilterExtensions {
        public static IReadOnlyList<T> Apply<T>(this ITableFilter<T>? filter, IEnumerable<T> items) {
            return filter == null ? items.ToArray() : items.Where(filter.Matches).ToArray();
        }
    }
}