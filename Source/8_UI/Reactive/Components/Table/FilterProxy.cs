using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.UI.Reactive.Components {
    internal class FilterProxy<T> : ITableFilter<T> {
        #region Proxy
        
        public FilterProxy() {
            _filters = new(
                HandleFilterAdded,
                HandleFilterRemoved,
                HandleAllFiltersRemoved
            );
        }

        public ICollection<ITableFilter<T>> Filters => _filters;

        private readonly ObservableSet<ITableFilter<T>> _filters;

        #endregion

        #region Filter

        public event Action? FilterUpdatedEvent;

        public bool Matches(T value) {
            return _filters.All(x => x.Matches(value));
        }

        #endregion

        #region Callbacks

        private void HandleFilterAdded(ITableFilter<T> filter) {
            filter.FilterUpdatedEvent += HandleFilterUpdated;
        }

        private void HandleFilterRemoved(ITableFilter<T> filter) {
            filter.FilterUpdatedEvent -= HandleFilterUpdated;
        }

        private void HandleAllFiltersRemoved(IEnumerable<ITableFilter<T>> filters) {
            foreach (var filter in filters) {
                HandleFilterRemoved(filter);
            }
        }

        private void HandleFilterUpdated() {
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}